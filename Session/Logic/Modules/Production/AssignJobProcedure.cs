using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AssignJobProcedure : Procedure
{
    public Dictionary<Vector2, int> AssignmentDeltas { get; private set; }
    public List<JobAssignment> NewAssignments { get; private set; }
    public AssignJobProcedure(Dictionary<Vector2, int> assignmentDeltas, List<JobAssignment> newAssignments)
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
        foreach (var kvp in AssignmentDeltas)
        {
            var peep = key.Data.Society.Peeps[(int)kvp.Key.x];
            peep.ChangeJobAssignmentCount((byte)kvp.Key.y, kvp.Value, key);
        }
        foreach (var ja in NewAssignments)
        {
            var peep = ja.Peep.Entity();
            peep.AddJobAssignment(ja, key);
        }
    }
}
