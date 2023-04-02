// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using Godot;
//
// public class AssignJobModule : LogicModule
// {
//     public override void Calculate(Data data, Action<Message> queueMessage, 
//         Action<Func<HostWriteKey, Entity>> queueEntityCreation)
//     {
//         var proc = AssignJobProcedure.Construct();
//         foreach (var poly in data.Planet.Polygons.Entities)
//         {
//             HandlePoly(poly, data, data.Models.PeepJobs.Models.Values.ToList(), proc);
//         }
//         queueMessage(proc);
//     }
//
//     private void HandlePoly(MapPolygon poly, Data data, List<PeepJob> jobs, AssignJobProcedure proc)
//     {
//         var peeps = poly.GetPeeps(data);
//         if (peeps == null) return;
//         var unemployedPeeps = peeps.Where(p => p.GetUnemployedCount() > 0);
//         var buildings = poly.GetBuildings(data);
//         if (buildings == null) return;
//             
//         var workersNeededBuildings = buildings
//             .Where(b => b.Model.Model() is WorkBuildingModel wb)
//             .Select(b => (WorkBuildingModel)b.Model.Model());
//         if (workersNeededBuildings.Count() == 0) return;
//             
//         var jobNeeds = workersNeededBuildings
//             .SelectMany(b => b.JobLaborReqs)
//             .SortInto(b => b.Key, b => b.Value);
//
//         var unemployeds = unemployedPeeps.ToDictionary(p => p, p => p.GetUnemployedCount());
//         var sw = new Stopwatch();
//         // sw.Start();
//         GrowExistingAssignments();
//         // sw.Stop();
//         // GD.Print(nameof(GrowExistingAssignments) + " " + sw.Elapsed.TotalMilliseconds);
//         // sw.Reset();
//
//         // sw.Start();
//         MakeNewAssignments();
//         // sw.Stop();
//         // GD.Print(nameof(MakeNewAssignments) + " " + sw.Elapsed.TotalMilliseconds);
//         // sw.Reset();
//         
//         void GrowExistingAssignments()
//         {
//             for (var i = 0; i < jobs.Count; i++)
//             {
//                 var job = jobs[i];
//                 if (jobNeeds.ContainsKey(job) == false) continue;
//                 while (jobNeeds[job] > 0)
//                 {
//                     var hasRelevant = unemployeds.Any(kvp2 => kvp2.Key.Jobs.ContainsKey(job));
//                     if (hasRelevant == false) break;
//                     var relevant = unemployeds.First(kvp2 => kvp2.Key.Jobs.ContainsKey(job));
//                     var hire = Mathf.Min(jobNeeds[job], relevant.Value);
//                     jobNeeds[job] -= hire;
//                     unemployeds[relevant.Key] -= hire;
//                     if (unemployeds[relevant.Key] == 0)
//                     {
//                         unemployeds.Remove(relevant.Key);
//                     }
//                     proc.AssignmentDeltas.Add(
//                     new Tuple<int, string, int>
//                         (relevant.Key.Id, job.Name, hire)
//                     );
//                 }
//             }
//         }
//
//         void MakeNewAssignments()
//         {
//             for (var i = 0; i < jobs.Count; i++)
//             {
//                 var job = jobs[i];
//                 if (jobNeeds.ContainsKey(job) == false) continue;
//                 while (jobNeeds[job] > 0 && unemployeds.Count > 0)
//                 {
//                     var unemployed = unemployeds.First();
//                     var hire = Mathf.Min(jobNeeds[job], unemployed.Value);
//                     unemployeds[unemployed.Key] -= hire;
//                     if (unemployeds[unemployed.Key] == 0) unemployeds.Remove(unemployed.Key);
//                     jobNeeds[job] -= hire;
//                     var ja = new JobAssignment(unemployed.Key.MakeRef(),
//                         job.MakeRef(), hire, 100);
//                     proc.NewAssignments.Add(ja);
//                 }
//             }
//         }
//     }
// }
