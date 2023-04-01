using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AssignJobModule : LogicModule
{
    public override void Calculate(Data data, Action<Message> queueMessage, 
        Action<Func<HostWriteKey, Entity>> queueEntityCreation)
    {
        var assignmentDeltas = new Dictionary<Vector2, int>();
        var newAssignments = new List<JobAssignment>();

        foreach (var poly in data.Planet.Polygons.Entities)
        {
            var peeps = poly.GetPeeps(data);
            if (peeps == null) continue;
            var unemployedPeeps = peeps.Where(p => p.GetUnemployedCount() > 0);
            
            
            var buildings = poly.GetBuildings(data);
            if (buildings == null) continue;
            var workersNeededBuildings = buildings
                .Where(b => b.Model.Model() is WorkBuildingModel wb && wb.PeepsLaborReq - b.NumWorkers(data) > 0);
            if (workersNeededBuildings.Count() == 0) continue;
            var jobNeeds = workersNeededBuildings
                .ToDictionary(
                    b => b, 
                    b => ((WorkBuildingModel) b.Model.Model()).PeepsLaborReq - b.NumWorkers(data));
            
            var unemployeds = unemployedPeeps
                .SortInto(p => p, p => p.GetUnemployedCount());

            var relevantJas = unemployedPeeps
                .SelectMany(p => p.Jobs.Values
                    .Where(ja => ja.Unemployed() == false 
                                 && jobNeeds.ContainsKey(ja.Building.Entity()))
                );
            foreach (var ja in relevantJas)
            {
                var building = ja.Building.Entity();
                var peep = ja.Peep.Entity();
                
                var unemployed = unemployeds[peep];
                if (unemployed == 0) continue;

                if (jobNeeds.ContainsKey(building) == false) continue;
                var jobNeed = jobNeeds[building];
                
                var hire = Mathf.Min(jobNeed, unemployed);
                unemployeds[peep] -= hire;
                if (unemployeds[peep] < 0) continue;

                jobNeeds[building] -= hire;
                if (jobNeeds[building] == 0) jobNeeds.Remove(building);
                assignmentDeltas.Add(new Vector2(peep.Id, ja.Marker), hire);
            }
            
            foreach (var peep in unemployedPeeps)
            {
                var unemployed = unemployeds[peep];
                while (unemployed > 0 && jobNeeds.Count > 0)
                {
                    var kvp = jobNeeds.First();
                    var building = kvp.Key;
                    var job = ((WorkBuildingModel) building.Model.Model()).JobType;
                    var need = kvp.Value;
                    var hire = Mathf.Min(need, unemployed);
                    if (hire < 0) throw new Exception();
                    jobNeeds[building] -= hire;
                    if (jobNeeds[building] == 0)
                    {
                        jobNeeds.Remove(building);
                    }
                    unemployed -= hire;
                    var ja = new JobAssignment(peep.MakeRef(), 
                        255, 
                        job.MakeRef(), 
                        building.MakeRef(),
                        hire, 100);
                    newAssignments.Add(ja);
                }
            }
        }

        var assignLabor = new AssignJobProcedure(assignmentDeltas, newAssignments);
        queueMessage(assignLabor);
    }
}
