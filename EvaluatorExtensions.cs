using Notes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Xml.Linq;
using Systools;

namespace WCF
{
  /// <summary>  
  /// Extensions for the evaluator.
  /// </summary>  
  public partial class EvaluatorExtensions
  {
    public static Func<String, object> GetSystemConfigParameter;

    public static Func<String, CStorageLocation, bool> IsInAttribute;
    public static Func<String, CWob> GetWob;
    public class WrongNumberOfParameters : Exception
    {
      public WrongNumberOfParameters(String Message, int Number)
        : base($"{Message} (Excpected: {Number}).")
      {
      }
    }
    /// <summary>
    /// Adds note information to the given evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="s">The note to add to the evaluator.</param>
    public static void AddNote(Evaluator ev, CNote s)
    {
      ev.AddObject("Note.Type", s.m_NoteType);
      ev.AddObject("Note.Text", s.m_Content);
    }

    /// <summary>
    /// Determines whether a note of the given type exists for a material or not.
    /// </summary>
    /// <param name="Type">The note type.</param>
    /// <param name="MaterialID">The material.</param>
    /// <returns>A value indicating whether a note of the given type exists for a material or not.</returns>
    public delegate bool GetNote(String Type, String MaterialID);

    /// <summary>
    /// Adds the TryGetNote-Function to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="del">The delegate to get the note information.</param>
    public static void AddTryGetNote(Evaluator ev, GetNote del)
    {
      ev.AddFunction("TryGetNote", new XElement("Function", new XAttribute("Result", "Boolean"),
        new XAttribute("Example", "TryGetNote(s1,s2)<br>TryGetNote('@|TR|Restriction|TR|@', '123456')"),
        new XAttribute("Name", "TryGetNote"),
        new XAttribute("Description", "@|TR|Try to get a note of type s1 for material s2|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "s1")),
        new XElement("Parameter", new XAttribute("Material", "String"), new XAttribute("Name", "s2"))),
          o =>
          {
            if (o.Length != 2)
              throw new Exception("~2 Parameters expected in TryGetNote");

            if (!(o[0] is String))
              throw new Exception("~Parameter has to be a String");

            if (!(o[1] is String))
              throw new Exception("~Parameter has to be a String");

            if (del != null)
              return del(o[0] as String, o[1] as String);

            return true;
          });
      ev.AddFunction("IsForRail", new XElement("Function", new XAttribute("Result", "Boolean"),
           new XAttribute("Example", "IsForRail(s1,s2)"),
           new XAttribute("Name", "IsForRail"),
           new XAttribute("Description", "@|TR|Returns true if s1 is planned for a rail car on rail s2 ('ST21-01-R1' for example)|TR|@"),
           new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "s1")),
           new XElement("Parameter", new XAttribute("Material", "String"), new XAttribute("Name", "s2"))),
             o =>
             {
               if (o.Length != 2)
                 throw new Exception("~2 Parameters expected in IsForRail");

               if (!(o[0] is String))
                 throw new Exception("~Parameter has to be a String");
               if (!(o[1] is String))
                 throw new Exception("~Parameter has to be a String");

               if (string.IsNullOrEmpty(o[0] as string))
                 return false;
               if (string.IsNullOrEmpty(o[1] as string))
                 return false;


               return del != null && del(CNoteTypes.RAIL_DISPATCH + "-" + o[1].ToString(), o[0].ToString());
             });

      ev.AddFunction("IsForBargeShipping", new XElement("Function", new XAttribute("Result", "Boolean"),
       new XAttribute("Example", "IsForBargeShipping(s1)"),
       new XAttribute("Name", "IsForBargeShipping"),
       new XAttribute("Description", "@|TR|Returns true if s1 is a shipping order number for barge shipping|TR|@"),
       new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "s1"))),
         o =>
         {
           if (o.Length != 1)
             throw new Exception("~1 Parameters expected in IsForBargeShipping");

           if (!(o[0] is String))
             throw new Exception("~Parameter has to be a String");

           if (string.IsNullOrEmpty(o[0] as string))
             return false;

           if (del != null)
           {
             del(CNoteTypes.BARGE_SHIPPING, "General");

             var nt = ev.GetObject("Note.Type") as string;
             if (nt != null && nt == CNoteTypes.BARGE_SHIPPING)
             {
               var content = WCFToString.Convert<List<String>>(ev.GetObject("Note.Text") as string);
               return content.Contains(o[0]);
             }
             return false;

           }

           return false;
         });

      ev.AddFunction("BatchAnnealingForward", new XElement("Function", new XAttribute("Result", "Boolean"),
 new XAttribute("Example", "BatchAnnealingForward()"),
 new XAttribute("Name", "BatchAnnealingForward"),
 new XAttribute("Description", "@|TR|Returns true if operation mode for BA is in forward mode|TR|@")),
   o =>
   {
     if (o.Length != 0)
       throw new Exception("~No Parameters expected in BatchAnnealingForward");

     if (del != null)
     {
       del(CNoteTypes.BATCH_ANNEALING_FWD, "General");

       var nt = ev.GetObject("Note.Type") as string;
       if (nt != null && nt == CNoteTypes.BATCH_ANNEALING_FWD)
       {
         return ev.GetObject("Note.Text") as string == "FWD";
       }
       return false;

     }

     return false;
   });

      ev.AddFunction("PackingPriority", new XElement("Function", new XAttribute("Result", "String"),
new XAttribute("Example", "PackingPriority()"),
new XAttribute("Name", "PackingPriority"),
new XAttribute("Description", "@|TR|Returns Normal/Barge/Truck as string|TR|@")),
o =>
{
  if (o.Length != 0)
    throw new Exception("~No Parameters expected in PackingPriority");

  if (del != null)
  {
    del(CNoteTypes.PACKING_PRIORITY, "General");

    var nt = ev.GetObject("Note.Type") as string;
    if (nt != null && nt == CNoteTypes.PACKING_PRIORITY)
    {
      return ev.GetObject("Note.Text") as string;
    }
    return "";

  }
  return "";
});
    }


    /// <summary>
    /// Exception that is raised whenever a parameter with the wrong type has been passed into the evaluator.
    /// </summary>
    /// <seealso cref="System.Exception" />
    protected class WrongParameterType : Exception
    {
      public WrongParameterType(String Message, String ParameterName, Type ExceptcedType) :
        base($"{Message} ({ParameterName}: {ExceptcedType})")
      {
      }
    }

    /// <summary>
    /// Adds default WCF functionalities to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    public static void AddWCFFunctionality(Evaluator ev)
    {
      foreach (var item in Enum.GetValues(typeof(CWob.DataStateEnum)))
        ev.AddBasicObject(item.ToString(), item);

      CWob.RegisterWobParameters(ev);

      ev.AddFunction("MakeStorageLocation", new XElement("Function", new XAttribute("Result", "CStorageLocation"),
        new XAttribute("Example", "MakeStorageLocation('B7-7B4A-17')"),
        new XAttribute("Name", "MakeStorageLocation"),
        new XAttribute("Description", "@|TR|Tries to parse a storage location from a string|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "a1"))),
        o =>
        {
          if (ev.TestMode)
            return CStorageLocation.SimpleParse("B7");
          return CStorageLocation.SimpleParse((string)o[0]);
        });

      ev.AddFunction("ConvertToDecimal", new XElement("Function", new XAttribute("Result", "decimal"),
             new XAttribute("Example", "ConvertToDecimal('AB123')"),
             new XAttribute("Name", "ConvertToDecimal"),
             new XAttribute("Description", "@|TR|Returns a number converted from string (A=1,...Z=26) in a string|TR|@"),
             new XElement("Parameter", new XAttribute("Type", "string"), new XAttribute("Name", "a1"))),
             o =>
             {
               if (o.Length != 1)
                 throw new WrongNumberOfParameters("~1 Parameter expected in ConvertToDecimal", 1);
               if (o[0] as string == null)
                 throw new WrongParameterType("~Parameter must be of type string", "a1", typeof(string));

               var str = o[0] as string;
               int val = 0;
               for (int i = 0; i < str.Length; i++)
               {
                 if (str[i] >= '0' && str[i] <= '9') { val *= 10; val += str[i] - '0'; }
                 if (str[i] >= 'A' && str[i] <= 'Z') { val *= 100; val += str[i] - 'A' + 1; }
                 if (str[i] >= 'a' && str[i] <= 'z') { val *= 100; val += str[i] - 'a' + 1; }
               }
               return (decimal)val;
             });
      ev.AddFunction("GetRowname", new XElement("Function", new XAttribute("Result", "String"),
             new XAttribute("Example", "GetRowname(Coil.LagerortVorLastaufnahme)"),
             new XAttribute("Name", "GetRowname"),
             new XAttribute("Description", "@|TR|Gibt den Reihennamen zurück|TR|@"),
             new XElement("Parameter", new XAttribute("Type", "CStorageLocation"), new XAttribute("Name", "a1"))),
             o =>
             {
               if (o.Length != 1)
                 throw new WrongNumberOfParameters("~1 Parameter expected in Rowname", 1);
               if (o[0] as CStorageLocation == null)
                 throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
               return (o[0] as CStorageLocation).m_Row;
             });

      ev.AddFunction("GetAreaname", new XElement("Function", new XAttribute("Result", "String"),
       new XAttribute("Example", "GetAreaname(Coil.LagerortVorLastaufnahme)"),
       new XAttribute("Name", "GetAreaname"),
       new XAttribute("Description", "@|TR|Gibt den Bereichsnamen zurück|TR|@"),
       new XElement("Parameter", new XAttribute("Type", "CStorageLocation"), new XAttribute("Name", "a1"))),
       o =>
       {
         if (o.Length != 1)
           throw new WrongNumberOfParameters("~1 Parameter expected in Areaname", 1);
         if (o[0] as CStorageLocation == null)
           throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
         return (o[0] as CStorageLocation).m_Area;
       });

      ev.AddFunction("GetBayname", new XElement("Function", new XAttribute("Result", "String"),
       new XAttribute("Example", "GetBayname(Coil.LagerortVorLastaufnahme)"),
       new XAttribute("Name", "GetBayname"),
       new XAttribute("Description", "@|TR|Gibt den Hallennamen zurück|TR|@"),
       new XElement("Parameter", new XAttribute("Type", "CStorageLocation"), new XAttribute("Name", "a1"))),
       o =>
       {
         if (o.Length != 1)
           throw new WrongNumberOfParameters("~1 Parameter expected in Bayname", 1);
         if (o[0] as CStorageLocation == null)
           throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
         return (o[0] as CStorageLocation).m_Bay;
       });

      ev.AddFunction("GetLocation", new XElement("Function", new XAttribute("Result", "int"),
        new XAttribute("Example", "GetLocation(Coil.LagerortVorLastaufnahme)"),
        new XAttribute("Name", "GetLocation"),
        new XAttribute("Description", "@|TR|Gibt den Platz zurück|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "CStorageLocation"), new XAttribute("Name", "a1"))),
        o =>
        {
          if (o.Length != 1)
            throw new WrongNumberOfParameters("~1 Parameter expected in Rowname", 1);
          if (o[0] as CStorageLocation == null)
            throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
          return (o[0] as CStorageLocation).m_Location;
        });

      ev.AddFunction("GetLayer", new XElement("Function", new XAttribute("Result", "int"),
        new XAttribute("Example", "GetLayer(Coil.LagerortVorLastaufnahme)"),
        new XAttribute("Name", "GetLayer"),
        new XAttribute("Description", "@|TR|Gibt die Lage zurück|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "CStorageLocation"), new XAttribute("Name", "a1"))),
        o =>
        {
          if (o.Length != 1)
            throw new WrongNumberOfParameters("~1 Parameter expected in Rowname", 1);
          if (o[0] as CStorageLocation == null)
            throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
          return (o[0] as CStorageLocation).m_Layer;
        });

      ev.AddFunction("MakeVehicleStorageLocation", new XElement("Function", new XAttribute("Result", "CStorageLocation"),
         new XAttribute("Example", "MakeVehicleStorageLocation('UN-C-392, 1')"),
         new XAttribute("Name", "MakeVehicleStorageLocation"),
         new XAttribute("Description", "@|TR|Creates a vehicle storage location|TR|@"),
                new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "v")),
                new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "t"))),
         o =>
         {
           return new CStorageLocation(CStorageLocation.StorageLocationType.VEHICLE) { m_VehicleID = (string)o[0], m_PositionInVehicle = (string)o[1] };
         });

      ev.AddFunction("ReduceStorageLocationToRow", new XElement("Function", new XAttribute("Result", "CStorageLocation"),
        new XAttribute("Example", "ReduceStorageLocationToRow(@|TR|Coil|TR|@.@|TR|StorageLocation|TR|@)"),
        new XAttribute("Name", "ReduceStorageLocationToRow"),
        new XAttribute("Description", "@|TR|Reduces a storage location to row detail level|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "CStorageLocation"), new XAttribute("Name", "s1"))),
        o =>
        {
          if (o.Length != 1)
            throw new Exception("~1 Parameter expected in ReduceStorageLocationToRow");

          return ((CStorageLocation)o[0]).ReduceToRow();
        });

      AddIsInAttribute(ev);

      ev.AddFunction("IsAtCrane", new XElement("Function", new XAttribute("Result", "Boolean"),
        new XAttribute("Example", "IsAtCrane(s1,k1)<br>IsAtCrane(@|TR|Coil|TR|@.@|TR|StorageLocation|TR|@, '18')"),
        new XAttribute("Name", "IsAtCrane"),
        new XAttribute("Description", "@|TR|Returns true if s1 is at MOT k1|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "CStorageLocation"), new XAttribute("Name", "s1")),
        new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "k1"))),
          o =>
          {
            if (o.Length != 2)
              throw new Exception("~2 Parameter expected in IsAtCrane");

            if (!((CStorageLocation)o[0]).IsCraneStorage)
              return false;

            return ((CStorageLocation)o[0]).m_Row == (String)o[1];
          });

      ev.AddFunction("GetX", new XElement("Function", new XAttribute("Result", "Decimal"),
        new XAttribute("Example", "GetX(c)<br>GetX(@|TR|Coil|TR|@.@|TR|Coordinates|TR|@)"),
        new XAttribute("Name", "GetX"),
        new XAttribute("Description", "@|TR|Returns the x value of a coordinate|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "CCoord3D"), new XAttribute("Name", "c"))),
          o => (decimal)((CCoord3D)o[0]).m_X);

      ev.AddFunction("GetY", new XElement("Function", new XAttribute("Result", "Decimal"),
        new XAttribute("Example", "GetY(c)<br>GetY(@|TR|Coil|TR|@.@|TR|Coordinates|TR|@)"),
        new XAttribute("Name", "GetY"),
        new XAttribute("Description", "@|TR|Returns the y value of a coordinate|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "CCoord3D"), new XAttribute("Name", "c"))),
          o => (decimal)((CCoord3D)o[0]).m_Y);

      ev.AddFunction("GetZ", new XElement("Function", new XAttribute("Result", "Decimal"),
        new XAttribute("Example", "GetZ(c)<br>GetZ(@|TR|Coil|TR|@.@|TR|Coordinates|TR|@)"),
        new XAttribute("Name", "GetZ"),
        new XAttribute("Description", "@|TR|Returns the z value of a coordinate|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "CCoord3D"), new XAttribute("Name", "c"))),
          o => (decimal)((CCoord3D)o[0]).m_Z);

      ev.AddFunction("ParseInt", new XElement("Function", new XAttribute("Result", "Decimal"),
        new XAttribute("Example", "ParseInt(c)<br>ParseInt('123')"),
        new XAttribute("Name", "ParseInt"),
        new XAttribute("Description", "@|TR|Interprets the given string as an integer|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "c"))),
          o =>
          {
            var f = (String)o[0];
            int i;
            int.TryParse(f, out i);
            return (decimal)i;
          });

      ev.AddFunction("Round", new XElement("Function",
        new XAttribute("Example", "Round(123.7) => 123"),
        new XAttribute("Result", "Decimal"),
        new XAttribute("Name", "Round"),
        new XAttribute("Description", "Rounds the given decimal"),
        new XElement("Parameter", new XAttribute("Type", "decimal"), new XAttribute("Name", "p1"))),
        o =>
        {
          if (o.Length == 1)
            return decimal.Round((decimal)o[0]);
          return decimal.Round((decimal)o[0], (int)(decimal)o[1]);
        });

      ev.AddFunction("YesNo", new XElement("Function",
        new XAttribute("Example", "YesNo(true)) => 'Y'"),
        new XAttribute("Result", "String"),
        new XAttribute("Name", "YesNo"),
        new XAttribute("Description", "Converts the given bool to Yes/No"),
        new XElement("Parameter", new XAttribute("Type", "Boolean"), new XAttribute("Name", "p1"))),
        o =>
        {
          return o[0].ToString().ToLower() == "true" ? "Yes" : "No";
        });



      AddMaximumNumberOfTransports(ev);
    }



    public delegate bool IsOnShortTermSequence(string unitname);
    public static void AddIsOnSequence(Evaluator ev, IsOnShortTermSequence del = null)
    {
      ev.AddFunction("IsOnShortTermSequence",
                        new XElement("Function", new XAttribute("Result", "Boolean"),
                        new XAttribute("Example", "IsOnShortTermSequence('HSM')"),
                        new XAttribute("Name", "IsOnShortTermSequence"),
                        new XAttribute("Description", "Returns true, if the material is on the short term sequence of the specified unit."),
                        new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "unit"))), delegate (Object[] o)
                        {
                          if (o.Length != 1)
                            throw new Exception("~1 Parameter expected in IsOnShortTermSequence");

                          if (del != null)
                            return del(o[0] as string);

                          return false;
                        });


      ev.AddFunction("IsOnLongTermSequence",
                       new XElement("Function", new XAttribute("Result", "Boolean"),
                       new XAttribute("Example", "IsOnLongTermSequence('HSM')"),
                       new XAttribute("Name", "IsOnLongTermSequence"),
                       new XAttribute("Description", "Returns true, if the material is on the long term sequence of the specified unit."),
                       new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "unit"))), delegate (Object[] o)
                       {
                         if (o.Length != 1)
                           throw new Exception("~1 Parameter expected in IsOnLongTermSequence");

                         if (del != null)
                           return del(o[0] as string);

                         return false;
                       });
    }

    /// <summary>
    /// Adds historical transport information to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="tr">The historical transport information.</param>
    public static void AddSingleTransport(Evaluator ev, CSingleTransport tr)
    {
      ev.AddObject("Coil." + CWobIdent.NameEvaluatorMaterialID, tr.m_WobIdent.m_MaterialID);

      if (CWobIdent.HasSecondID)
        ev.AddObject("Coil." + CWobIdent.NameEvaluatorSecondID, tr.m_WobIdent.m_SecondID);
      if (CWobIdent.HasThirdID)
        ev.AddObject("Coil." + CWobIdent.NameEvaluatorThirdID, tr.m_WobIdent.m_ThirdID);

      ev.AddObject("Coil.Weight", tr.m_Weight);
      ev.AddObject("Transport.Timestamp", tr.m_Timestamp);
      ev.AddObject("Transport.Duration", tr.m_TransportTime);
      ev.AddObject("Transport.MOTID", tr.m_MotID);
      ev.AddObject("Transport.MOTName", tr.m_MotName);
      ev.AddObject("Transport.RestrictionViolation", tr.m_RestrictionViolation);

      ev.AddObject("Source.StorageLocation", tr.m_LocationFrom);
      if (CStorageLocation.DISPLAY_MAINAREA)
        ev.AddObject("Source.MainArea", tr.m_LocationFrom.m_MainArea);

      if (CStorageLocation.DISPLAY_BAY)
        ev.AddObject("Source.Bay", tr.m_LocationFrom.m_Bay);

      if (CStorageLocation.DISPLAY_AREA)
        ev.AddObject("Source.Area", tr.m_LocationFrom.m_Area);

      if (CStorageLocation.DISPLAY_ROW)
        ev.AddObject("Source.Row", tr.m_LocationFrom.m_Row);

      if (CStorageLocation.DISPLAY_LOCATION)
        ev.AddObject("Source.Location", tr.m_LocationFrom.m_Location);

      if (CStorageLocation.DISPLAY_LAYER)
        ev.AddObject("Source.Layer", tr.m_LocationFrom.m_Layer);

      ev.AddObject("Source.VehicleID", tr.m_LocationFrom.m_VehicleID);
      ev.AddObject("Source.PositionInVehicle", tr.m_LocationFrom.m_PositionInVehicle);
      ev.AddObject("Source.Attribute", tr.m_LocationFromAttribute);
      ev.AddObject("Source.CoordMOT", tr.m_CoordFromCrane);
      ev.AddObject("Source.CoordMOT.X", tr.m_CoordFromCrane.m_X);
      ev.AddObject("Source.CoordMOT.Y", tr.m_CoordFromCrane.m_Y);
      ev.AddObject("Source.CoordMOT.Z", tr.m_CoordFromCrane.m_Z);
      ev.AddObject("Source.CoordTWMS", tr.m_CoordFromLVS);
      ev.AddObject("Source.CoordTWMS.X", tr.m_CoordFromLVS.m_X);
      ev.AddObject("Source.CoordTWMS.Y", tr.m_CoordFromLVS.m_Y);
      ev.AddObject("Source.CoordTWMS.Z", tr.m_CoordFromLVS.m_Z);

      ev.AddObject("Destination.StorageLocation", tr.m_LocationTo);
      if (CStorageLocation.DISPLAY_MAINAREA)
        ev.AddObject("Destination.MainArea", tr.m_LocationTo.m_MainArea);

      if (CStorageLocation.DISPLAY_BAY)
        ev.AddObject("Destination.Bay", tr.m_LocationTo.m_Bay);

      if (CStorageLocation.DISPLAY_AREA)
        ev.AddObject("Destination.Area", tr.m_LocationTo.m_Area);

      if (CStorageLocation.DISPLAY_ROW)
        ev.AddObject("Destination.Row", tr.m_LocationTo.m_Row);

      if (CStorageLocation.DISPLAY_LOCATION)
        ev.AddObject("Destination.Location", tr.m_LocationTo.m_Location);

      if (CStorageLocation.DISPLAY_LAYER)
        ev.AddObject("Destination.Layer", tr.m_LocationTo.m_Layer);

      ev.AddObject("Destination.VehicleID", tr.m_LocationTo.m_VehicleID);
      ev.AddObject("Destination.PositionInVehicle", tr.m_LocationTo.m_PositionInVehicle);
      ev.AddObject("Destination.Attribute", tr.m_LocationToAttribute);
      ev.AddObject("Destination.CoordMOT", tr.m_CoordToCrane);
      ev.AddObject("Destination.CoordMOT.X", tr.m_CoordToCrane.m_X);
      ev.AddObject("Destination.CoordMOT.Y", tr.m_CoordToCrane.m_Y);
      ev.AddObject("Destination.CoordMOT.Z", tr.m_CoordToCrane.m_Z);
      ev.AddObject("Destination.CoordTWMS", tr.m_CoordToLVS);
      ev.AddObject("Destination.CoordTWMS.X", tr.m_CoordToLVS.m_X);
      ev.AddObject("Destination.CoordTWMS.Y", tr.m_CoordToLVS.m_Y);
      ev.AddObject("Destination.CoordTWMS.Z", tr.m_CoordToLVS.m_Z);

      ev.AddObject("Suggestion.StorageLocation", tr.m_SuggestedLocation);
      if (CStorageLocation.DISPLAY_MAINAREA)
        ev.AddObject("Suggestion.MainArea", tr.m_SuggestedLocation.m_MainArea);

      if (CStorageLocation.DISPLAY_BAY)
        ev.AddObject("Suggestion.Bay", tr.m_SuggestedLocation.m_Bay);

      if (CStorageLocation.DISPLAY_AREA)
        ev.AddObject("Suggestion.Area", tr.m_SuggestedLocation.m_Area);

      if (CStorageLocation.DISPLAY_ROW)
        ev.AddObject("Suggestion.Row", tr.m_SuggestedLocation.m_Row);

      if (CStorageLocation.DISPLAY_LOCATION)
        ev.AddObject("Suggestion.Location", tr.m_SuggestedLocation.m_Location);

      if (CStorageLocation.DISPLAY_LAYER)
        ev.AddObject("Suggestion.Layer", tr.m_SuggestedLocation.m_Layer);

      ev.AddObject("Suggestion.VehicleID", tr.m_SuggestedLocation.m_VehicleID);
      ev.AddObject("Suggestion.PositionInVehicle", tr.m_SuggestedLocation.m_PositionInVehicle);
      ev.AddObject("Suggestion.Coord", tr.m_SuggestedCoord);
      ev.AddObject("Suggestion.CoordX", tr.m_SuggestedCoord.m_X);
      ev.AddObject("Suggestion.CoordY", tr.m_SuggestedCoord.m_Y);
      ev.AddObject("Suggestion.CoordZ", tr.m_SuggestedCoord.m_Z);
    }

    /// <summary>
    /// Adds transport order information to an evalutaor.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="t">The transport order information.</param>
    public static void AddTransportOrder(Evaluator ev, CTransportOrder t)
    {
      var sl = CStorageLocation.Example;


      ev.AddObject("Order.CreationDate", t.m_CreationDate);

      var cl = t.CurrentLastDestination;
      ev.AddObject("Order.CurrentDestination", cl == null ? "" : cl.SubsToDisplayString);
      ev.AddObject("Order.ForeignOrderID", t.m_ForeignOrderID);
      ev.AddObject("Order.IsForcedOrder", t.m_ForeignOrderID != 0);
      ev.AddObject("Order.LastChange", t.m_LastChange);
      ev.AddObject("Order.OrderCategories", t.m_OrderCategories);
      ev.AddObject("Order.Originator", t.m_Originator);
      ev.AddObject("Order.Priority", t.Priority);
      ev.AddObject("Order.Rulename", t.m_RuleName);
      ev.AddObject("Order.Status", t.m_Status);
      ev.AddObject("Order.Comment", t.m_StatusComment);
      ev.AddObject("Order.MOTID", t.m_MOTID);
      ev.AddObject("Order.TOMID", t.m_TOMID);
      ev.AddObject("Order.Hint", t.m_Hint);

      if (t.m_Destinations != null)
      {
        ev.AddObject("CurrentDestination.StorageLocation", t.CurrentDestination.m_StorageLocation);
        ev.AddObject("CurrentDestination.MainArea", t.CurrentDestination.m_StorageLocation.m_MainArea);
        ev.AddObject("CurrentDestination.Bay", t.CurrentDestination.m_StorageLocation.m_Bay);
        ev.AddObject("CurrentDestination.Area", t.CurrentDestination.m_StorageLocation.m_Area);
        ev.AddObject("CurrentDestination.Row", t.CurrentDestination.m_StorageLocation.m_Row);
        ev.AddObject("CurrentDestination.Location", t.CurrentDestination.m_StorageLocation.m_Location);
        ev.AddObject("CurrentDestination.Layer", t.CurrentDestination.m_StorageLocation.m_Layer);
        ev.AddObject("CurrentDestination.VehicleID", t.CurrentDestination.m_StorageLocation.m_VehicleID);
        ev.AddObject("CurrentDestination.PositionInVehicle", t.CurrentDestination.m_StorageLocation.m_PositionInVehicle);
        ev.AddObject("CurrentDestination.Attribute", t.CurrentDestination.m_AttributeName ?? "");
      }
      else
      {
        ev.AddObject("CurrentDestination.StorageLocation", sl);
        ev.AddObject("CurrentDestination.MainArea", sl.m_MainArea);
        ev.AddObject("CurrentDestination.Bay", sl.m_Bay);
        ev.AddObject("CurrentDestination.Area", sl.m_Area);
        ev.AddObject("CurrentDestination.Row", sl.m_Row);
        ev.AddObject("CurrentDestination.Location", sl.m_Location);
        ev.AddObject("CurrentDestination.Layer", sl.m_Layer);
        ev.AddObject("CurrentDestination.VehicleID", sl.m_VehicleID);
        ev.AddObject("CurrentDestination.PositionInVehicle", sl.m_PositionInVehicle);
        ev.AddObject("CurrentDestination.Attribute", "");
      }

      if (t.m_Destinations != null && t.m_Destinations.Count != 0)
      {
        var last = t.m_Destinations.Last();
        if (last != null)
        {
          ev.AddObject("FinalDestination.StorageLocation", last.m_StorageLocation);
          ev.AddObject("FinalDestination.MainArea", last.m_StorageLocation.m_MainArea);
          ev.AddObject("FinalDestination.Bay", last.m_StorageLocation.m_Bay);
          ev.AddObject("FinalDestination.Area", last.m_StorageLocation.m_Area);
          ev.AddObject("FinalDestination.Row", last.m_StorageLocation.m_Row);
          ev.AddObject("FinalDestination.Location", last.m_StorageLocation.m_Location);
          ev.AddObject("FinalDestination.Layer", last.m_StorageLocation.m_Layer);
          ev.AddObject("FinalDestination.VehicleID", last.m_StorageLocation.m_VehicleID);
          ev.AddObject("FinalDestination.PositionInVehicle", last.m_StorageLocation.m_PositionInVehicle);
          ev.AddObject("FinalDestination.Attribute", last.m_AttributeName ?? "");
        }
      }
      else
      {
        ev.AddObject("FinalDestination.StorageLocation", sl);
        ev.AddObject("FinalDestination.Bay", sl.m_Bay);
        ev.AddObject("FinalDestination.Area", sl.m_Area);
        ev.AddObject("FinalDestination.Row", sl.m_Row);
        ev.AddObject("FinalDestination.Location", sl.m_Location);
        ev.AddObject("FinalDestination.Layer", sl.m_Layer);
        ev.AddObject("FinalDestination.VehicleID", sl.m_VehicleID);
        ev.AddObject("FinalDestination.PositionInVehicle", sl.m_PositionInVehicle);
        ev.AddObject("FinalDestination.Attribute", "");
      }
    }

    /// <summary>
    /// Adds historical strorage location information to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="e">The historical storage location information.</param>
    public static void AddLocationHistoryEntry(Evaluator ev, CLocationHistoryEntry e)
    {
      ev.AddObject("Coil." + CWobIdent.NameEvaluatorMaterialID, e.m_WobIdent.m_MaterialID);

      if (CWobIdent.HasSecondID)
        ev.AddObject("Coil." + CWobIdent.NameEvaluatorSecondID, e.m_WobIdent.m_SecondID);
      if (CWobIdent.HasThirdID)
        ev.AddObject("Coil." + CWobIdent.NameEvaluatorThirdID, e.m_WobIdent.m_ThirdID);

      ev.AddObject("Event.Timestamp", e.m_EventDate);
      ev.AddObject("Event.Type", e.m_EventType);
      ev.AddObject("Event.Originator", String.IsNullOrEmpty(e.m_Originator.Name) ? "" : e.m_Originator.Name);
      ev.AddObject("StorageLocation", e.m_StorageLocation);

      // ---- 'Event.Originator' is only added where it is needed ----

      if (CStorageLocation.DISPLAY_MAINAREA)
        ev.AddObject("StorageLocation.MainArea", e.m_StorageLocation.m_MainArea);

      if (CStorageLocation.DISPLAY_BAY)
        ev.AddObject("StorageLocation.Bay", e.m_StorageLocation.m_Bay);

      if (CStorageLocation.DISPLAY_AREA)
        ev.AddObject("StorageLocation.Area", e.m_StorageLocation.m_Area);

      if (CStorageLocation.DISPLAY_ROW)
        ev.AddObject("StorageLocation.Row", e.m_StorageLocation.m_Row);

      if (CStorageLocation.DISPLAY_LOCATION)
        ev.AddObject("StorageLocation.Location", e.m_StorageLocation.m_Location);

      if (CStorageLocation.DISPLAY_LAYER)
        ev.AddObject("StorageLocation.Layer", e.m_StorageLocation.m_Layer);

      ev.AddObject("StorageLocation.VehicleID", e.m_StorageLocation.m_VehicleID);
      ev.AddObject("StorageLocation.PositionInVehicle", e.m_StorageLocation.m_PositionInVehicle);
    }

    /// <summary>
    /// Adds system message information to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="s">The system message information.</param>
    public static void AddSystemMessage(Evaluator ev, CSystemMessage s)
    {
      if (s.m_ID == 0)
      {
        s.m_WobIdent = new CWobIdent();
        s.m_Text = "";
        s.m_ID = 0;
        s.m_Number = 0;
        s.m_Originator = "";
        s.m_TimeStamp = DateTime.Now;
        s.m_Type = MsgType.INFO;
        s.m_RestrictionSeverity = CStorageOptimizationRuleElement.RestrictionType.NONE;
      }

      ev.AddObject("Coil." + CWobIdent.NameEvaluatorMaterialID, s.m_WobIdent.m_MaterialID);

      if (CWobIdent.HasSecondID)
        ev.AddObject("Coil." + CWobIdent.NameEvaluatorSecondID, s.m_WobIdent.m_SecondID);
      if (CWobIdent.HasThirdID)
        ev.AddObject("Coil." + CWobIdent.NameEvaluatorThirdID, s.m_WobIdent.m_ThirdID);

      ev.AddObject("SystemMessage.ID", s.m_ID);
      ev.AddObject("SystemMessage.Number", s.m_Number);
      ev.AddObject("SystemMessage.Originator", s.m_Originator);
      ev.AddObject("SystemMessage.Text", s.m_Text);
      ev.AddObject("SystemMessage.Timestamp", s.m_TimeStamp);
      ev.AddObject("SystemMessage.Type", s.m_Type);
      ev.AddObject("SystemMessage.RestrictionType", s.m_RestrictionSeverity);
    }

    /// <summary>
    /// Adds restriction violation information to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="s">The restriction violation information.</param>
    public static void AddRestrictionViolation(Evaluator ev, CSystemMessage s)
    {
      if (s.m_ID == 0)
      {
        s.m_WobIdent = new CWobIdent();
        s.m_Text = "";
        s.m_Originator = "";
        s.m_TimeStamp = DateTime.Now;
        s.m_RestrictionSeverity = CStorageOptimizationRuleElement.RestrictionType.NONE;
      }

      ev.AddObject("Coil." + CWobIdent.NameEvaluatorMaterialID, s.m_WobIdent.m_MaterialID);

      if (CWobIdent.HasSecondID)
        ev.AddObject("Coil." + CWobIdent.NameEvaluatorSecondID, s.m_WobIdent.m_SecondID);
      if (CWobIdent.HasThirdID)
        ev.AddObject("Coil." + CWobIdent.NameEvaluatorThirdID, s.m_WobIdent.m_ThirdID);

      ev.AddObject("SystemMessage.Originator", s.m_Originator);
      ev.AddObject("SystemMessage.Text", s.m_Text);
      ev.AddObject("SystemMessage.Timestamp", s.m_TimeStamp);
      ev.AddObject("SystemMessage.RestrictionType", s.m_RestrictionSeverity);
    }

    /// <summary>
    /// Adds a list of defined attributes to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="attributes">The list of defined attributes.</param>
    public static void SetAttributes(Evaluator ev, List<CAttribute> attributes)
    {
      ev.ClearAttributes();
      foreach (var a in attributes)
        ev.AddAttribute(a.m_Attribute);
    }

    /// <summary>
    /// Adds information about the current TWMS client instance to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="session">The current client session.</param>
    /// <param name="tt">The current terminal type.</param>
    public static void AddClient(Evaluator ev, CSession session, TerminalType tt)
    {
      AddTerminal(ev, session.m_TermNo, session.m_Invoker, tt);
    }

    /// <summary>
    /// Adds terminal information to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="termNo">The current terminal number of the TWMS client.</param>
    /// <param name="user">The name of the currently registered user.</param>
    /// <param name="tt">The current terminal type.</param>
    public static void AddTerminal(Evaluator ev, int termNo, String user, TerminalType tt)
    {
      ev.AddObject("Client.TermNo", termNo);
      ev.AddObject("Client.User", user);
      ev.AddObject("Client.TerminalType", tt);
    }

    /// <summary>
    /// Adds the IsInAttribute-Function to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="delIsInAttribute">A delegate to determine whether a storage location is within an attribute or not.</param>
    public static void AddIsInAttribute(Evaluator ev)
    {
      ev.AddFunction("IsInAttribute", new XElement("Function", new XAttribute("Result", "Boolean"),
        new XAttribute("Example", "IsInAttribute(s1,a1)<br>IsInAttribute(@|TR|Coil|TR|@.@|TR|StorageLocation|TR|@, '@|TR|Main storage|TR|@')"),
        new XAttribute("Name", "IsInAttribute"),
        new XAttribute("Description", "@|TR|Returns true if s1 is located within attribute a1|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "CStorageLocation"), new XAttribute("Name", "s1")),
        new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "a1"))),
          o =>
          {
            if (o.Length != 2)
              throw new Exception("~2 Parameter expected in IsInAttribute");

            CStorageLocation sl = (CStorageLocation)o[0];
            String a1 = (String)o[1];

            if (IsInAttribute != null)
              return IsInAttribute(a1, sl);

            return false;
          });
    }

    /// <summary>
    /// Adds information about the system drives to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="d">The drive information.</param>
    public static void AddDriveInfo(Evaluator ev, DriveInfo d, Func<String, String> translate)
    {
      if (d == null)
      {
        var space = 1000000;
        ev.AddObject("HardDisk.TotalSize", space.ToString(), translate?.Invoke("~The total disk space in bytes."));
        ev.AddObject("HardDisk.TotalSizeMB", Math.Round(space / 1024d / 1024d, 2).ToString(), translate?.Invoke("~The total disk space in megabytes."));
        ev.AddObject("HardDisk.TotalSizeGB", Math.Round(space / 1024d / 1024d / 1024d, 2).ToString(), translate?.Invoke("~The total disk space in gigabytes."));

        ev.AddObject("HardDisk.TotalFreeSpace", space.ToString(), translate?.Invoke("~The free disk space in bytes."));
        ev.AddObject("HardDisk.TotalFreeSpaceMB", Math.Round(space / 1024d / 1024d, 2).ToString(), translate?.Invoke("~The free disk space in megabytes."));
        ev.AddObject("HardDisk.TotalFreeSpaceGB", Math.Round(space / 1024d / 1024d / 1024d, 2).ToString(), translate?.Invoke("~The free disk space in gigabytes."));

        ev.AddObject("HardDisk.Name", @"C:\", translate?.Invoke("~The name of the hard disk"));
        ev.AddObject("HardDisk.VolumeLabel", translate?.Invoke("~Data"));
      }
      else
      {
        ev.AddObject("HardDisk.TotalSize", d.TotalSize.ToString(), translate?.Invoke("~The total disk space in bytes."));
        ev.AddObject("HardDisk.TotalSizeMB", Math.Round(d.TotalSize / 1024d / 1024d, 2).ToString(), translate?.Invoke("~The total disk space in megabytes."));
        ev.AddObject("HardDisk.TotalSizeGB", Math.Round(d.TotalSize / 1024d / 1024d / 1024d, 2).ToString(), translate?.Invoke("~The total disk space in gigabytes."));

        ev.AddObject("HardDisk.TotalFreeSpace", d.TotalFreeSpace.ToString(), translate?.Invoke("~The free disk space in bytes."));
        ev.AddObject("HardDisk.TotalFreeSpaceMB", Math.Round(d.TotalFreeSpace / 1024d / 1024d, 2).ToString(), translate?.Invoke("~The free disk space in megabytes."));
        ev.AddObject("HardDisk.TotalFreeSpaceGB", Math.Round(d.TotalFreeSpace / 1024d / 1024d / 1024d, 2).ToString(), translate?.Invoke("~The free disk space in gigabytes."));

        ev.AddObject("HardDisk.Name", d.Name, translate?.Invoke("~The name of the hard disk"));
        ev.AddObject("HardDisk.VolumeLabel", d.VolumeLabel);
      }
    }

    /// <summary>
    /// Adds capacity information for a storage location to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="sl">The storage location.</param>
    /// <param name="free">The number of free locations in the storage location.</param>
    /// <param name="capacity">The maximum capacity of the storage location.</param>
    public static void AddStorageLocationInfo(Evaluator ev, CStorageLocation sl, int free, int capacity)
    {
      ev.AddObject("StorageLocation.Name", sl.DisplayString());
      ev.AddObject("StorageLocation.Free", free.ToString());
      ev.AddObject("StorageLocation.Capacity", capacity.ToString());
    }

    /// <summary>
    /// Adds wob parameters depending on the given wob types to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="wobTypes">The wob types to add.</param>
    /// <param name="sampleBay">A sample bay.</param>
    public static void AddWobParameters(Evaluator ev, List<CWob.WobType> wobTypes, String sampleBay = "")
    {
      if (String.IsNullOrEmpty(sampleBay))
        sampleBay = CStorageLocation.Example.m_Bay;

      wobTypes.ForEach(wt =>
      {
        switch (wt)
        {
          case CWob.WobType.COIL:
            {
              var wob = new CCoil();
              wob.InitSampleValues(sampleBay);
              CWob.AddWobParameters(wob, ev);
              break;
            }
          case CWob.WobType.SLAB:
            {
              var wob = new CSlab();
              wob.InitSampleValues(sampleBay);
              CWob.AddWobParameters(wob, ev);
              break;
            }
          case CWob.WobType.BILLET:
          case CWob.WobType.BLOOM:
          case CWob.WobType.PLATE:
          case CWob.WobType.UNDEFINED:
          default:
            break;
        }
      });
    }

    /// <summary>
    /// Adds wob parameters depending on the given wob type to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="wobType">The wob type to add.</param>
    /// <param name="sampleBay">A sample bay.</param>
    /// <returns>The new created wob.</returns>
    public static CWob AddWobParameters(Evaluator ev, CWob.WobType wobType, String sampleBay = "")
    {
      if (String.IsNullOrEmpty(sampleBay))
        sampleBay = CStorageLocation.Example.m_Bay;

      switch (wobType)
      {
        case CWob.WobType.COIL:
          {
            var wob = new CCoil();
            wob.InitSampleValues(sampleBay);
            CWob.AddWobParameters(wob, ev);
            return wob;
          }
        case CWob.WobType.SLAB:
          {
            var wob = new CSlab();
            wob.InitSampleValues(sampleBay);
            CWob.AddWobParameters(wob, ev);
            return wob;
          }
        case CWob.WobType.BILLET:
        case CWob.WobType.BLOOM:
        case CWob.WobType.PLATE:
        case CWob.WobType.UNDEFINED:
        default:
          return null;
      }
    }

    /// <summary>
    /// Adds capacity information for a statisticgroup to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    /// <param name="statisticgroup">The name of the statisticgroup.</param>
    /// <param name="sector">The name of the sektor.</param>
    /// <param name="description">The name of the sektor.</param>
    /// <param name="free">The number of free locations in the bay.</param>
    /// <param name="capacity">The maximum capacity of the bay.</param>
    public static void AddStatisticGroup(Evaluator ev, String statisticgroup, String sector, String description, int free, int capacity)
    {
      ev.AddObject("StatisticGroup.StatisticGroup", statisticgroup);
      //ev.AddObject("StatisticGroup.Sector", sector, "Statistikgruppe.Sektor");
      ev.AddObject("StatisticGroup.Description", description);
      ev.AddObject("StatisticGroup.Free", free.ToString());
      ev.AddObject("StatisticGroup.Capacity", capacity.ToString());
    }

    public static void AddMOTName(Evaluator ev, String mot)
    {
      ev.AddObject("MOT.Name", mot);
    }

    public static void AddStorageRestriction(Evaluator ev, CStorageRestriction restriction, String originator, DateTime timestamp, Func<String, String> translator = null)
    {
      ev.AddObject("SystemMessage.Text", restriction.ToLog(t => translator != null ? translator(t) : t));
      ev.AddObject("SystemMessage.Originator", originator);
      ev.AddObject("SystemMessage.Timestamp", timestamp);
      ev.AddObject("SystemMessage.RestrictionType", restriction.m_Severity);
    }

    public static void AddMaximumNumberOfTransports(Evaluator ev)
    {
      var obj = GetSystemConfigParameter?.Invoke("MAXIMUM_REPOSITIONING_TRANSPORTS");
      if (obj == null || (obj is int) == false)
        ev.AddBasicObject("MaximumNumberOfTransports", int.MaxValue);
      else
        ev.AddBasicObject("MaximumNumberOfTransports", (int)obj);
    }
  }

  /// <summary>
  /// Contains evaluator extensions regarding the capacity of a bay.
  /// </summary>
  public partial class CCapacityEvaluatorDummies
  {
    /// <summary>
    /// Adds dummy functions for capacity evaluation to an evaluator.
    /// </summary>
    /// <param name="ev">The evaluator to extend.</param>
    public static void AddEvaluatorDummies(Evaluator ev)
    {


      ev.AddFunction("GetCoilUtilization", new XElement("Function", new XAttribute("Result", "Decimal"),
        new XAttribute("Example", "GetCoilUtilization(b)<br>GetCoilUtilization('21PP')"),
        new XAttribute("Name", "GetCoilUtilization"),
        new XAttribute("Description", "@|TR|Returns coil utilization in statistic group b|TR|@"),
        new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "b"))),
          o =>
          {
            if (o.Length != 1)
              throw new Exception("~1 Parameter expected in GetCoilUtilization");

            return 0;
          });
    }
  }


  [DataContract]
  public partial class CDynamicValues
  {

    public enum TYPE { INT, STRING, BOOL };

    [DataMember] public string m_MainArea;
    [DataMember] public string m_Bay;
    [DataMember] public CompareRuleSetsEnum m_Rulebook;
    [DataMember] public string m_Key;
    [DataMember] public TYPE m_Type;
    [DataMember] public string m_Value;
    [DataMember] public string m_Description;

    public CStorageLocation Bay
    {
      get
      {
        return new CStorageLocation { m_MainArea = m_MainArea, m_Bay = m_Bay };
      }

    }
    public object Value
    {
      get
      {
        switch (m_Type)
        {
          case TYPE.INT:
            return int.Parse(m_Value);
          case TYPE.STRING:
            return m_Value;
          case TYPE.BOOL:
            return m_Value == "Y" || m_Value == "J" || m_Value.ToLower() == "true";
        }
        return null;
      }
    }
  }
  /// <summary>
  /// Interface for the WMS-Server contract.
  /// </summary>
  public partial interface IWMSContract
  {
    /// <summary>
    /// reads all dynamic values from server
    /// </summary>
    /// <param name="session"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    [OperationContract]
    CErrCode ReadDynamicValues(CSession session, out List<CDynamicValues> values);
  }
}