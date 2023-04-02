using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AssignJobProcedure : Procedure
{
    public List<Tuple<int, string, int>> AssignmentDeltas { get; private set; }
    public List<JobAssignment> NewAssignments { get; private set; }

    public static AssignJobProcedure Construct()
    {
        var assignmentDeltas = new List<Tuple<int, string, int>>();
        var newAssignments = new List<JobAssignment>();
        return new AssignJobProcedure(assignmentDeltas, newAssignments);
    }
    public AssignJobProcedure(List<Tuple<int, string, int>> assignmentDeltas, 
        List<JobAssignment> newAssignments)
    {
        AssignmentDeltas = assignmentDeltas;
        NewAssignments = newAssignments;
    }
    
    public override bool Valid(Data data)
    {
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        foreach (var tuple in AssignmentDeltas)
        {
            var peep = key.Data.Society.Peeps[tuple.Item1];
            var jobType = key.Data.Models.PeepJobs.Models[tuple.Item2];
            
            peep.ChangeJobAssignmentCount(jobType, tuple.Item3, key);
        }
        foreach (var ja in NewAssignments)
        {
            var peep = ja.Peep.Entity();
            peep.AddJobAssignment(ja, key);
        }
    }
}
