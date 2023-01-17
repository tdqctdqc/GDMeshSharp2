using Godot;
using System;

public class EntityProcedureAttribute : Attribute
{
    /*
    
    Must only either use SetByProcedure or ProcedureSetField on entities variables,
    or call other entities procedures
    Procedure will store whichever ProcedureSetField delegates it needs for itself
     
     */
}
