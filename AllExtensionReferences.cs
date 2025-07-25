      "IsDestination", ("Function", ("Result", "Boolean"),
				("Example", "IsDestination(d1,a1)<br>IsDestination(@|TR|Destination|TR|@, '@|TR|Main storage|TR|@')"),
        ("Name", "IsDestination"),
        ("Description", "@|TR|Returns true if d1 is in attribute a1|TR|@"),
        ("Parameter", ("Type", "CDestination"), ("Name", "d1")),
        ("Parameter", ("Type", "String"), ("Name", "a1"))),


      "GetFloorLoad", ("Function",
      ("Example", "GetFloorLoad(Coord)"),
      ("Result", "Decimal"),
      ("Name", "GetFloorLoad"),
      ("Description", "@|TR|Gets the load in kg on the given coordinate c1|TR|@"),
      ("Parameter", ("Type", "Coord3D"), ("Name", "c1"))),

    "LoadExceedsFloorLoad", ("Function",
    ("Example", "LoadExceedsFloorLoad(@|TR|Coordinates|TR|@, @|TR|Layer|TR|@, @|TR|Coil|TR|@.@|TR|Weight|TR|@, 25000)"),
    ("Result", "Decimal"),
    ("Name", "LoadExceedsFloorLoad"),
    ("Description", "@|TR|Returns true if the given floor load is exceeded after placing coil with given properties|TR|@"),
    ("Parameter", ("Type", "Coord3D"), ("Name", "@|TR|Coordinates|TR|@")),
    ("Parameter", ("Type", "decimal"), ("Name", "@|TR|Layer|TR|@")),
    ("Parameter", ("Type", "decimal"), ("Name", "@|TR|Weight|TR|@")),
    ("Parameter", ("Type", "decimal"), ("Name", "@|TR|MaxLoad|TR|@"))
    ),

    "CalcDistance", ("Function",
    ("Example", "CalcDistance(p1,p2)<br>CalcDistance(@|TR|Coil|TR|@.@|TR|Coordinates|TR|@, @|TR|Coordinates|TR|@)"),
    ("Result", "Decimal"),
    ("Name", "CalcDistance"),
    ("Parameter", ("Type", "Coord3D"), ("Name", "p1")),
    ("Parameter", ("Type", "Coord3D"), ("Name", "p2")),
    ("Description", "@|TR|Provides distance between p1 and p2|TR|@")),

    "CalcSupporter", ("Function",
    ("Example", ""),
    ("Result", "Boolean"),
    ("Name", "CalcSupporter"),
    ("Description", "@|TR|Provides supporting materials|TR|@")), o => true);

      "CalcNeighbor", ("Function",
      ("Example", "CalcNeighbor(n1)<br>CalcNeighbor(1)<br>CalcNeighbor(2)"),
      ("Result", "Boolean"),
      ("Name", "CalcNeighbor"),
      ("Description", "@|TR|Provides neighbor materials|TR|@"),
      ("Parameter", ("Type", "decimal"), ("Name", "n1"))), delegate (Object[] o)


      "CalcDistanceDijkstra", ("Function",
      ("Example", "CalcDistanceDijkstra()"),
      ("Result", "Decimal"),
      ("Name", "CalcDistanceDijkstra"),
      ("Description", "Berechnung der Distanz zwischen Start und Ziel nach Dijkstra.")),



      "GetCoilUtilization", ("Function", ("Result", "Decimal"),
     ("Example", "GetCoilUtilization(b)<br>GetCoilUtilization('21PP')"),
     ("Name", "GetCoilUtilization"),
     ("Description", "@|TR|Returns coil utilization|TR|@"),
     ("Parameter", ("Type", "String"), ("Name", "Statisticgroup"))


      "CalcSupporter", ("Function",
      ("Example", ""),
      ("Result", "Boolean"),
      ("Name", "CalcSupporter"),
      ("Description", "@|TR|Provides supporting materials|TR|@")), o => SupportingCount > 0);

      "CalcNeighbor", ("Function",
      ("Example", "CalcNeighbor(n1)<br>CalcNeighbor(1)<br>CalcNeighbor(2)"),
      ("Result", "Boolean"),
      ("Name", "CalcNeighbor"),
      ("Description", "@|TR|Provides neighbor materials|TR|@"),
      ("Parameter", ("Type", "decimal"), ("Name", "n1"))), delegate (Object[] o)



      "IsInAttribute", null, delegate (Object[] o)
      {
        if (o.Length != 2)
          throw new Exception("~2 Parameter expected in IsInAttribute");

        CStorageLocation sl = (CStorageLocation)o[0];
        String a1 = (String)o[1];

        return m_AttributeCacheLock.IsFullFilled(a1, sl);
      });

      "CalcDistance", null, delegate (Object[] o)
      {
        if (o.Length != 2)
          throw new Exception("~2 Parameter expected in CalcDistance");
        CCoord3D c1 = (CCoord3D)o[0];
        CCoord3D c2 = (CCoord3D)o[1];
        return (decimal)c1.DistXY(c2);
      });

      "CalcDistanceDijkstra", null, delegate
      {
        var wp = m_PreparedWaypointConnections;
        int idx = int.MaxValue;

        var targetNode = new CWaypoint()
        {
          m_ID = idx--,
          m_Coord = m_CurrentSL.m_Coord
        };

        var targetWP = GetConnections(wp, targetNode, m_PreparedSpecialZones);
        wp = wp.Concat(targetWP).ToList();

        var craneNode = new CWaypoint()
        {
          m_ID = idx--,
          m_Coord = m_PreparedWob.m_Coord
        };

        var motWP = GetConnections(wp, craneNode, m_PreparedSpecialZones);
        wp = wp.Concat(motWP).ToList();

        var currentDijkstra = new Dijkstra(wp);
        currentDijkstra.CalculateDistance(craneNode);

        return currentDijkstra.DistanceForRules(targetNode);
      });

      EvaluatorExtensions.AddNote(m_Evaluator, new CNote());
      EvaluatorExtensions.AddTryGetNote(m_Evaluator, (type, material) =>
      {
        List<CNote> lst = CNotes.Instance.GetNotes(material);

        var note = lst.Find(t => t.m_NoteType == type);
        if (note == null)
          return false;

        EvaluatorExtensions.AddNote(m_Evaluator, note);
        return true;
      });

      "CalcSupporter", null, delegate
      {
        return CalcSupporterResult;
      });

      "CalcNeighbor", ("x"), delegate (Object[] o)
      {
        var idx = (int)(decimal)o[0] - 1;
        if (idx == 0)
          return CalcNeigbor1Result;
        return CalcNeigbor2Result;
      });
      EvaluatorExtensions.AddIsOnSequence(m_Evaluator);


      "GetCoilUtilization", ("Function", ("Result", "Decimal"),
           ("Example", "GetCoilUtilization(b)<br>GetCoilUtilization('21PP')"),
           ("Name", "GetCoilUtilization"),
           ("Description", "@|TR|Returns coil utilization|TR|@"),
           ("Parameter", ("Type", "String"), ("Name", "Statisticgroup"))
           ),
             o =>
             {
               if (o.Length != 1)
                 throw new WrongNumberOfParameters("~1 Parameter expected in GetCoilUtilization", 2);

               var statNeu = m_StockStatistics.Where(t => t.m_StatisticGroup.m_StatisticGroup == o[0] as String).ToList();
               if (statNeu == null || statNeu.Count <= 0)
                 return (decimal)100.0;

               double fgr = 0.0;
               foreach (var s in statNeu)
               {
                 fgr = fgr + (100.0 - (100.0 * (((double)s.m_FirstLayerAvailableLocations) / ((double)s.m_TotalLocations))));
               }
               fgr = fgr / (double)statNeu.Count;
               return (decimal)fgr;
             });

      StockStatisticsReRead.AutoReset = false;
      StockStatisticsReRead.Interval = 120000;
      StockStatisticsReRead.Elapsed += StockStatisticsReRead_Elapsed;
      StockStatisticsReRead.Start();
    }

      ev.AddFunction("TryGetNote", ("Function", ("Result", "Boolean"),
        ("Example", "TryGetNote(s1,s2)<br>TryGetNote('@|TR|Restriction|TR|@', '123456')"),
        ("Name", "TryGetNote"),
        ("Description", "@|TR|Try to get a note of type s1 for material s2|TR|@"),
        ("Parameter", ("Type", "String"), ("Name", "s1")),
        ("Parameter", ("Material", "String"), ("Name", "s2"))),
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
      ev.AddFunction("IsForRail", ("Function", ("Result", "Boolean"),
           ("Example", "IsForRail(s1,s2)"),
           ("Name", "IsForRail"),
           ("Description", "@|TR|Returns true if s1 is planned for a rail car on rail s2 ('ST21-01-R1' for example)|TR|@"),
           ("Parameter", ("Type", "String"), ("Name", "s1")),
           ("Parameter", ("Material", "String"), ("Name", "s2"))),
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

      ev.AddFunction("IsForBargeShipping", ("Function", ("Result", "Boolean"),
       ("Example", "IsForBargeShipping(s1)"),
       ("Name", "IsForBargeShipping"),
       ("Description", "@|TR|Returns true if s1 is a shipping order number for barge shipping|TR|@"),
       ("Parameter", ("Type", "String"), ("Name", "s1"))),
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

      ev.AddFunction("BatchAnnealingForward", ("Function", ("Result", "Boolean"),
 ("Example", "BatchAnnealingForward()"),
 ("Name", "BatchAnnealingForward"),
 ("Description", "@|TR|Returns true if operation mode for BA is in forward mode|TR|@")),
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

      ev.AddFunction("PackingPriority", ("Function", ("Result", "String"),
("Example", "PackingPriority()"),
("Name", "PackingPriority"),
("Description", "@|TR|Returns Normal/Barge/Truck as string|TR|@")),
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

      ev.AddFunction("MakeStorageLocation", ("Function", ("Result", "CStorageLocation"),
        ("Example", "MakeStorageLocation('B7-7B4A-17')"),
        ("Name", "MakeStorageLocation"),
        ("Description", "@|TR|Tries to parse a storage location from a string|TR|@"),
        ("Parameter", ("Type", "String"), ("Name", "a1"))),
        o =>
        {
          if (ev.TestMode)
            return CStorageLocation.SimpleParse("B7");
          return CStorageLocation.SimpleParse((string)o[0]);
        });

      ev.AddFunction("ConvertToDecimal", ("Function", ("Result", "decimal"),
             ("Example", "ConvertToDecimal('AB123')"),
             ("Name", "ConvertToDecimal"),
             ("Description", "@|TR|Returns a number converted from string (A=1,...Z=26) in a string|TR|@"),
             ("Parameter", ("Type", "string"), ("Name", "a1"))),
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
      ev.AddFunction("GetRowname", ("Function", ("Result", "String"),
             ("Example", "GetRowname(Coil.LagerortVorLastaufnahme)"),
             ("Name", "GetRowname"),
             ("Description", "@|TR|Gibt den Reihennamen zurück|TR|@"),
             ("Parameter", ("Type", "CStorageLocation"), ("Name", "a1"))),
             o =>
             {
               if (o.Length != 1)
                 throw new WrongNumberOfParameters("~1 Parameter expected in Rowname", 1);
               if (o[0] as CStorageLocation == null)
                 throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
               return (o[0] as CStorageLocation).m_Row;
             });

      ev.AddFunction("GetAreaname", ("Function", ("Result", "String"),
       ("Example", "GetAreaname(Coil.LagerortVorLastaufnahme)"),
       ("Name", "GetAreaname"),
       ("Description", "@|TR|Gibt den Bereichsnamen zurück|TR|@"),
       ("Parameter", ("Type", "CStorageLocation"), ("Name", "a1"))),
       o =>
       {
         if (o.Length != 1)
           throw new WrongNumberOfParameters("~1 Parameter expected in Areaname", 1);
         if (o[0] as CStorageLocation == null)
           throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
         return (o[0] as CStorageLocation).m_Area;
       });

      ev.AddFunction("GetBayname", ("Function", ("Result", "String"),
       ("Example", "GetBayname(Coil.LagerortVorLastaufnahme)"),
       ("Name", "GetBayname"),
       ("Description", "@|TR|Gibt den Hallennamen zurück|TR|@"),
       ("Parameter", ("Type", "CStorageLocation"), ("Name", "a1"))),
       o =>
       {
         if (o.Length != 1)
           throw new WrongNumberOfParameters("~1 Parameter expected in Bayname", 1);
         if (o[0] as CStorageLocation == null)
           throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
         return (o[0] as CStorageLocation).m_Bay;
       });

      ev.AddFunction("GetLocation", ("Function", ("Result", "int"),
        ("Example", "GetLocation(Coil.LagerortVorLastaufnahme)"),
        ("Name", "GetLocation"),
        ("Description", "@|TR|Gibt den Platz zurück|TR|@"),
        ("Parameter", ("Type", "CStorageLocation"), ("Name", "a1"))),
        o =>
        {
          if (o.Length != 1)
            throw new WrongNumberOfParameters("~1 Parameter expected in Rowname", 1);
          if (o[0] as CStorageLocation == null)
            throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
          return (o[0] as CStorageLocation).m_Location;
        });

      ev.AddFunction("GetLayer", ("Function", ("Result", "int"),
        ("Example", "GetLayer(Coil.LagerortVorLastaufnahme)"),
        ("Name", "GetLayer"),
        ("Description", "@|TR|Gibt die Lage zurück|TR|@"),
        ("Parameter", ("Type", "CStorageLocation"), ("Name", "a1"))),
        o =>
        {
          if (o.Length != 1)
            throw new WrongNumberOfParameters("~1 Parameter expected in Rowname", 1);
          if (o[0] as CStorageLocation == null)
            throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
          return (o[0] as CStorageLocation).m_Layer;
        });

      ev.AddFunction("MakeVehicleStorageLocation", ("Function", ("Result", "CStorageLocation"),
         ("Example", "MakeVehicleStorageLocation('UN-C-392, 1')"),
         ("Name", "MakeVehicleStorageLocation"),
         ("Description", "@|TR|Creates a vehicle storage location|TR|@"),
                ("Parameter", ("Type", "String"), ("Name", "v")),
                ("Parameter", ("Type", "String"), ("Name", "t"))),
         o =>
         {
           return new CStorageLocation(CStorageLocation.StorageLocationType.VEHICLE) { m_VehicleID = (string)o[0], m_PositionInVehicle = (string)o[1] };
         });

      ev.AddFunction("ReduceStorageLocationToRow", ("Function", ("Result", "CStorageLocation"),
        ("Example", "ReduceStorageLocationToRow(@|TR|Coil|TR|@.@|TR|StorageLocation|TR|@)"),
        ("Name", "ReduceStorageLocationToRow"),
        ("Description", "@|TR|Reduces a storage location to row detail level|TR|@"),
        ("Parameter", ("Type", "CStorageLocation"), ("Name", "s1"))),
        o =>
        {
          if (o.Length != 1)
            throw new Exception("~1 Parameter expected in ReduceStorageLocationToRow");

          return ((CStorageLocation)o[0]).ReduceToRow();
        });

      AddIsInAttribute(ev);

      ev.AddFunction("IsAtCrane", ("Function", ("Result", "Boolean"),
        ("Example", "IsAtCrane(s1,k1)<br>IsAtCrane(@|TR|Coil|TR|@.@|TR|StorageLocation|TR|@, '18')"),
        ("Name", "IsAtCrane"),
        ("Description", "@|TR|Returns true if s1 is at MOT k1|TR|@"),
        ("Parameter", ("Type", "CStorageLocation"), ("Name", "s1")),
        ("Parameter", ("Type", "String"), ("Name", "k1"))),
          o =>
          {
            if (o.Length != 2)
              throw new Exception("~2 Parameter expected in IsAtCrane");

            if (!((CStorageLocation)o[0]).IsCraneStorage)
              return false;

            return ((CStorageLocation)o[0]).m_Row == (String)o[1];
          });

      ev.AddFunction("GetX", ("Function", ("Result", "Decimal"),
        ("Example", "GetX(c)<br>GetX(@|TR|Coil|TR|@.@|TR|Coordinates|TR|@)"),
        ("Name", "GetX"),
        ("Description", "@|TR|Returns the x value of a coordinate|TR|@"),
        ("Parameter", ("Type", "CCoord3D"), ("Name", "c"))),
          o => (decimal)((CCoord3D)o[0]).m_X);

      ev.AddFunction("GetY", ("Function", ("Result", "Decimal"),
        ("Example", "GetY(c)<br>GetY(@|TR|Coil|TR|@.@|TR|Coordinates|TR|@)"),
        ("Name", "GetY"),
        ("Description", "@|TR|Returns the y value of a coordinate|TR|@"),
        ("Parameter", ("Type", "CCoord3D"), ("Name", "c"))),
          o => (decimal)((CCoord3D)o[0]).m_Y);

      ev.AddFunction("GetZ", ("Function", ("Result", "Decimal"),
        ("Example", "GetZ(c)<br>GetZ(@|TR|Coil|TR|@.@|TR|Coordinates|TR|@)"),
        ("Name", "GetZ"),
        ("Description", "@|TR|Returns the z value of a coordinate|TR|@"),
        ("Parameter", ("Type", "CCoord3D"), ("Name", "c"))),
          o => (decimal)((CCoord3D)o[0]).m_Z);

      ev.AddFunction("ParseInt", ("Function", ("Result", "Decimal"),
        ("Example", "ParseInt(c)<br>ParseInt('123')"),
        ("Name", "ParseInt"),
        ("Description", "@|TR|Interprets the given string as an integer|TR|@"),
        ("Parameter", ("Type", "String"), ("Name", "c"))),
          o =>
          {
            var f = (String)o[0];
            int i;
            int.TryParse(f, out i);
            return (decimal)i;
          });

      ev.AddFunction("Round", ("Function",
        ("Example", "Round(123.7) => 123"),
        ("Result", "Decimal"),
        ("Name", "Round"),
        ("Description", "Rounds the given decimal"),
        ("Parameter", ("Type", "decimal"), ("Name", "p1"))),
        o =>
        {
          if (o.Length == 1)
            return decimal.Round((decimal)o[0]);
          return decimal.Round((decimal)o[0], (int)(decimal)o[1]);
        });

      ev.AddFunction("YesNo", ("Function",
        ("Example", "YesNo(true)) => 'Y'"),
        ("Result", "String"),
        ("Name", "YesNo"),
        ("Description", "Converts the given bool to Yes/No"),
        ("Parameter", ("Type", "Boolean"), ("Name", "p1"))),
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
                        ("Function", ("Result", "Boolean"),
                        ("Example", "IsOnShortTermSequence('HSM')"),
                        ("Name", "IsOnShortTermSequence"),
                        ("Description", "Returns true, if the material is on the short term sequence of the specified unit."),
                        ("Parameter", ("Type", "String"), ("Name", "unit"))), delegate (Object[] o)
                        {
                          if (o.Length != 1)
                            throw new Exception("~1 Parameter expected in IsOnShortTermSequence");

                          if (del != null)
                            return del(o[0] as string);

                          return false;
                        });


      ev.AddFunction("IsOnLongTermSequence",
                       ("Function", ("Result", "Boolean"),
                       ("Example", "IsOnLongTermSequence('HSM')"),
                       ("Name", "IsOnLongTermSequence"),
                       ("Description", "Returns true, if the material is on the long term sequence of the specified unit."),
                       ("Parameter", ("Type", "String"), ("Name", "unit"))), delegate (Object[] o)
                       {
                         if (o.Length != 1)
                           throw new Exception("~1 Parameter expected in IsOnLongTermSequence");

                         if (del != null)
                           return del(o[0] as string);

                         return false;
                       });
    }

      ev.AddFunction("IsOnShortTermSequence",
                        ("Function", ("Result", "Boolean"),
                        ("Example", "IsOnShortTermSequence('HSM')"),
                        ("Name", "IsOnShortTermSequence"),
                        ("Description", "Returns true, if the material is on the short term sequence of the specified unit."),
                        ("Parameter", ("Type", "String"), ("Name", "unit"))), delegate (Object[] o)
                        {
                          if (o.Length != 1)
                            throw new Exception("~1 Parameter expected in IsOnShortTermSequence");

                          if (del != null)
                            return del(o[0] as string);

                          return false;
                        });


      ev.AddFunction("IsOnLongTermSequence",
                       ("Function", ("Result", "Boolean"),
                       ("Example", "IsOnLongTermSequence('HSM')"),
                       ("Name", "IsOnLongTermSequence"),
                       ("Description", "Returns true, if the material is on the long term sequence of the specified unit."),
                       ("Parameter", ("Type", "String"), ("Name", "unit"))), delegate (Object[] o)
                       {
                         if (o.Length != 1)
                           throw new Exception("~1 Parameter expected in IsOnLongTermSequence");

                         if (del != null)
                           return del(o[0] as string);

                         return false;
                       });
    }

      ev.AddFunction("IsInAttribute", ("Function", ("Result", "Boolean"),
        ("Example", "IsInAttribute(s1,a1)<br>IsInAttribute(@|TR|Coil|TR|@.@|TR|StorageLocation|TR|@, '@|TR|Main storage|TR|@')"),
        ("Name", "IsInAttribute"),
        ("Description", "@|TR|Returns true if s1 is located within attribute a1|TR|@"),
        ("Parameter", ("Type", "CStorageLocation"), ("Name", "s1")),
        ("Parameter", ("Type", "String"), ("Name", "a1"))),
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

      ev.AddFunction("GetCoilUtilization", ("Function", ("Result", "Decimal"),
        ("Example", "GetCoilUtilization(b)<br>GetCoilUtilization('21PP')"),
        ("Name", "GetCoilUtilization"),
        ("Description", "@|TR|Returns coil utilization in statistic group b|TR|@"),
        ("Parameter", ("Type", "String"), ("Name", "b"))),
          o =>
          {
            if (o.Length != 1)
              throw new Exception("~1 Parameter expected in GetCoilUtilization");

            return 0;
          });
    }

ev.AddFunction("Count", ("Function",
	("Result", "Decimal"),
	("Example", "Count('@|TR|Coil|TR|@.@|TR|Width|TR|@ > 1000') > 2)"),
	("Name", "Count"),
	("Description", "@|TR|Returns the number of true conditions for a list|TR|@"),
	("Parameter", ("Type", "String"), ("Name", "a1"))),
	(o) =>
	{
		if (o.Length != 1)
			throw new Exception("Only one parameter allowed!");
		String a1 = (String)o[0];
		int count = 0;
		var expr = ev.GetExpression(a1);
		var Candidates = (List<CCoil>)ev.GetObject(prefix + ".Coils");
		foreach (var item in Candidates)
		{
			CWob.AddWobParameters(item, ev);
			if ((bool)expr.Evaluate(ev))
				count++;
		}
		return count;
	});
}

      ev.AddFunction("IsCoilBlocked", ("Function",
          ("Example", "IsCoilBlocked(Coil.Ident)"),
          ("Result", "Boolean"),
          ("Name", "IsCoilBlocked"),
          ("Description", "@|TR|Returns whether a coil is blocked by another coil on top|TR|@"),
          ("Parameter", ("Type", "string"), ("Name", "ident"))),
      delegate (Object[] o)
      {
        var c = (string)o[0];
        if (del != null)
          return del(c);
        return false;
      });
    }

      ev.AddFunction("IsOnShortTermSequence", ("Function",
          ("Example", "IsOnShortTermSequence(Coil.Ident)"),
          ("Result", "Boolean"),
          ("Name", "IsOnShortTermSequence"),
          ("Description", "@|TR|Returns whether a coil is on a given short term sequence|TR|@"),
          ("Parameter", ("Type", "string"), ("Name", "ident")),
          ("Parameter", ("Type", "string"), ("Name", "unit"))),
      delegate (Object[] o)
      {
        var c1 = (string)o[0];
        var c2 = (string)o[1];
        if (del1 != null)
          return del1(c1,c2);
        return false;
      });

      ev.AddFunction("GetShortTermSequencePosition", ("Function",
          ("Example", "GetShortTermSequencePosition(Coil.Ident)"),
          ("Result", "decimal"),
          ("Name", "GetShortTermSequencePosition"),
          ("Description", "@|TR|Returns the position of a coil if it is on a given short term sequence - or 999 if it is not on the given sequence|TR|@"),
          ("Parameter", ("Type", "string"), ("Name", "ident")),
          ("Parameter", ("Type", "string"), ("Name", "unit"))),
      delegate (Object[] o)
      {
        var c1 = (string)o[0];
        var c2 = (string)o[1];
        if (del2 != null)
          return del2(c1, c2);
        return 999m;
      });
    }

    AddFunction("ToString", ("Function",
      ("Example", "ToString(p1)<br>ToString(123) => '123'"),
      ("Result", "String"),
      ("Name", "ToString"),
      ("Description", "@|TR|Converts the given object to String|TR|@"),
      ("Parameter", ("Type", "Object"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          return o[0].ToString();
        });

    AddFunction("TimeSpan.ToSeconds", ("Function",
      ("Example", "TimeSpan.ToSeconds(p1)"),
      ("Result", "Decimal"),
      ("Name", "TimeSpan.ToSeconds"),
      ("Description", "@|TR|Returns the time difference in seconds|TR|@"),
      ("Parameter", ("Type", "TimeSpan"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          return (decimal)((TimeSpan)o[0]).TotalSeconds;
        });

    AddFunction("TimeSpan.ToMinutes", ("Function",
      ("Example", "TimeSpan.ToMinutes(p1)"),
      ("Result", "Decimal"),
      ("Name", "TimeSpan.ToMinutes"),
      ("Description", "@|TR|Returns the time difference in minutes|TR|@"),
      ("Parameter", ("Type", "TimeSpan"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          return (decimal)((TimeSpan)o[0]).TotalMinutes;
        });

    AddFunction("TimeSpan.ToHours", ("Function",
      ("Example", "TimeSpan.ToHours(p1)"),
      ("Result", "Decimal"),
      ("Name", "TimeSpan.ToHours"),
      ("Description", "@|TR|Returns the time difference in hours|TR|@"),
      ("Parameter", ("Type", "TimeSpan"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          return (decimal)((TimeSpan)o[0]).TotalHours;
        });

    AddFunction("TimeSpan.ToDays", ("Function",
      ("Example", "TimeSpan.ToDays(p1)"),
      ("Result", "Decimal"),
      ("Name", "TimeSpan.ToDays"),
      ("Description", "@|TR|Returns the time difference in days|TR|@"),
      ("Parameter", ("Type", "TimeSpan"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          return (decimal)((TimeSpan)o[0]).TotalDays;
        });

    AddFunction("DateTime.Now", ("Function",
      ("Example", "DateTime.Now() => DateTime Object"),
      ("Result", "DateTime"),
      ("Name", "DateTime.Now"),
      ("Description", "@|TR|Returns the current date/time|TR|@")),
        delegate (Object[] o)
        {
          return DateTime.Now;
        });

    AddFunction("DateTime.Parse", ("Function",
      ("Example", "DateTime.Parse(p1)<br>DateTime.Parse('11.5.2009') => DateTime Object"),
      ("Result", "DateTime"),
      ("Name", "DateTime.Parse"),
      ("Description", "@|TR|Parses a string for a date/time.|TR|@"),
      ("Parameter", ("Type", "String"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          return DateTime.Parse(o[0].ToString());
        });

    AddFunction("HasValue", ("Function",
      ("Example", "HasValue(p1)<br>HasValue('06.12.2013') => True"),
      ("Result", "Boolean"),
      ("Name", "HasValue"),
      ("Description", "@|TR|Returns true if p1 has a date/time value.|TR|@"),
      ("Parameter", ("Type", "DateTime"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          DateTime d = (DateTime)o[0];
          return d > DateTime.MinValue;
        });

    AddFunction("ToDecimal", ("Function",
      ("Example", "ToDecimal(p1)<br>ToDecimal('123') => 123"),
      ("Result", "Decimal"),
      ("Name", "ToDecimal"),
      ("Description", "@|TR|Converts the given string to a decimal if possible|TR|@"),
      ("Parameter", ("Type", "String"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          decimal result = 0;
          decimal.TryParse((String)o[0], System.Globalization.NumberStyles.None | System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out result);
          return result;
        });

    AddFunction("Abs", ("Function",
      ("Example", "Abs(p1)<br>Abs(-7) => 7"),
      ("Result", "Decimal"),
      ("Name", "Abs"),
      ("Description", "@|TR|Returns the absolute value of a decimal value|TR|@"),
      ("Parameter", ("Type", "Decimal"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          decimal v = Evaluator.Expr.DecimalCast(o[0]);
          return Math.Abs(v);
        });

    AddFunction("Sqrt", ("Function",
      ("Example", "Sqrt(p1)<br>Sqrt(64) => 8"),
      ("Result", "Decimal"),
      ("Name", "Sqrt"),
      ("Description", "@|TR|Returns the square root|TR|@"),
      ("Parameter", ("Type", "Decimal"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          decimal v = Evaluator.Expr.DecimalCast(o[0]);
          if (v < 0) return 0;
          return (decimal)Math.Sqrt((double)v);
        });

    AddFunction("Pow", ("Function",
      ("Example", "Pow(p1)<br>Pow(8) => 64"),
      ("Result", "Decimal"),
      ("Name", "Pow"),
      ("Description", "@|TR|Squares the given digit|TR|@"),
      ("Parameter", ("Type", "Decimal"), ("Name", "p1"))),
        delegate (Object[] o)
        {
          decimal v = Evaluator.Expr.DecimalCast(o[0]);
          return (decimal)(v * v);
        });

    AddFunction("SubStr", ("Function",
      ("Example", "SubStr(s,@|TR|StartIdx|TR|@[,@|TR|StringLength|TR|@])<br>SubStr('ABC',1) => 'BC'<br>SubStr('ABC',0,1) => 'A'"),
      ("Result", "String"),
      ("Name", "SubStr"),
      ("Description", "@|TR|Returns a substring from a string.|TR|@"),
      ("Parameter", ("Type", "String"), ("Name", "s")),
      ("Parameter", ("Type", "Decimal"), ("Name", "@|TR|StartIdx|TR|@")),
      ("Parameter", ("Type", "Decimal"), ("Name", "[@|TR|StringLength|TR|@]"))),
        delegate (Object[] o)
        {
          String s1 = (String)o[0];
          int startpos = (int)Evaluator.Expr.DecimalCast(o[1]);

          if (startpos >= s1.Length)
            return "";

          if (o.Length == 3)
          {
            int Length = (int)Evaluator.Expr.DecimalCast(o[2]);

            if (Length + startpos >= s1.Length)
              return s1.Substring(startpos);

            return s1.Substring(startpos, Length);
          }
          else
          {
            return s1.Substring(startpos);
          }
        });

    AddFunction("Contains", ("Function",
      ("Example", "Contains(s1,s2)<br>Contains('ABCDE','BCD') => True"),
      ("Result", "Boolean"),
      ("Name", "Contains"),
      ("Description", "@|TR|Returns true if s1 contains s2.|TR|@"),
      ("Parameter", ("Type", "String"), ("Name", "s1")),
      ("Parameter", ("Type", "String"), ("Name", "s2"))),
        delegate (Object[] o)
        {
          String s1 = (String)o[0];
          String s2 = (String)o[1];
          return s1.Contains(s2);
        });

    AddFunction("StartsWith", ("Function",
      ("Example", "StartsWith(s1,s2)<br>StartsWith('ABCDE','AB') => True"),
      ("Result", "Boolean"),
      ("Name", "StartsWith"),
      ("Description", "@|TR|Returns true if s1 starts with s2.|TR|@"),
      ("Parameter", ("Type", "String"), ("Name", "s1")),
      ("Parameter", ("Type", "String"), ("Name", "s2"))),
        delegate (Object[] o)
        {
          String s1 = (String)o[0];
          String s2 = (String)o[1];
          return s1.StartsWith(s2);
        });

    AddFunction("EndsWith", ("Function",
      ("Example", "EndsWith(s1,s2)<br>EndsWith('ABCDE','DE') => True"),
      ("Result", "Boolean"),
      ("Name", "EndsWith"),
      ("Description", "@|TR|Returns true if s1 ends with s2.|TR|@"),
      ("Parameter", ("Type", "String"), ("Name", "s1")),
      ("Parameter", ("Type", "String"), ("Name", "s2"))),
        delegate (Object[] o)
        {
          String s1 = (String)o[0];
          String s2 = (String)o[1];
          return s1.EndsWith(s2);
        });

    AddFunction("In", ("Function",
      ("Example", "In(p1,p2,...)<br>In(1,3,5,7,1) => True<br>In(2,3,5,7,1) => False <br>"),
      ("Result", "Boolean"),
      ("Name", "In"),
      ("Description", "@|TR|Searches a list of strings/int (p2,...) for p1.|TR|@"),
      ("Parameter", ("Type", "String/Decimal"), ("Name", "p1")),
      ("Parameter", ("Type", "String/Decimal"), ("Name", "p2")),
      ("Parameter", ("Type", "String/Decimal"), ("Name", "..."))),
        delegate (Object[] o)
        {
          for (int i = 1; i < o.Length; i++)
          {
            if (o[0].Equals(o[i]))
              return true;
          }
          return false;
        });

		AddFunction("CalendarWeek", ("Function",
			("Example", String.Format("CalendarWeek() => {0}", GetCalenderWeek(DateTime.Now))),
			("Result", "Decimal"),
			("Name", "CalendarWeek"),
			("Description", "@|TR|Returns the current calendar week|TR|@")),
				delegate
				{
					return GetCalenderWeek(DateTime.Now);
				});
	}

	public static int GetCalenderWeek(DateTime dt)
	{
		int cw = (dt.DayOfYear / 7) + 1;
		if (cw == 53)
			cw = 1;
		return cw;
	}
}

      ev.AddFunction("TryGetNote", ("Function", ("Result", "Boolean"),
        ("Example", "TryGetNote(s1,s2)<br>TryGetNote('@|TR|Restriction|TR|@', '123456')"),
        ("Name", "TryGetNote"),
        ("Description", "@|TR|Try to get a note of type s1 for material s2|TR|@"),
        ("Parameter", ("Type", "String"), ("Name", "s1")),
        ("Parameter", ("Material", "String"), ("Name", "s2"))),
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
      ev.AddFunction("IsForRail", ("Function", ("Result", "Boolean"),
           ("Example", "IsForRail(s1,s2)"),
           ("Name", "IsForRail"),
           ("Description", "@|TR|Returns true if s1 is planned for a rail car on rail s2 ('ST21-01-R1' for example)|TR|@"),
           ("Parameter", ("Type", "String"), ("Name", "s1")),
           ("Parameter", ("Material", "String"), ("Name", "s2"))),
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

      ev.AddFunction("IsForBargeShipping", ("Function", ("Result", "Boolean"),
       ("Example", "IsForBargeShipping(s1)"),
       ("Name", "IsForBargeShipping"),
       ("Description", "@|TR|Returns true if s1 is a shipping order number for barge shipping|TR|@"),
       ("Parameter", ("Type", "String"), ("Name", "s1"))),
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

      ev.AddFunction("BatchAnnealingForward", ("Function", ("Result", "Boolean"),
 ("Example", "BatchAnnealingForward()"),
 ("Name", "BatchAnnealingForward"),
 ("Description", "@|TR|Returns true if operation mode for BA is in forward mode|TR|@")),
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

      ev.AddFunction("PackingPriority", ("Function", ("Result", "String"),
("Example", "PackingPriority()"),
("Name", "PackingPriority"),
("Description", "@|TR|Returns Normal/Barge/Truck as string|TR|@")),
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

      ev.AddFunction("MakeStorageLocation", ("Function", ("Result", "CStorageLocation"),
        ("Example", "MakeStorageLocation('B7-7B4A-17')"),
        ("Name", "MakeStorageLocation"),
        ("Description", "@|TR|Tries to parse a storage location from a string|TR|@"),
        ("Parameter", ("Type", "String"), ("Name", "a1"))),
        o =>
        {
          if (ev.TestMode)
            return CStorageLocation.SimpleParse("B7");
          return CStorageLocation.SimpleParse((string)o[0]);
        });

      ev.AddFunction("ConvertToDecimal", ("Function", ("Result", "decimal"),
             ("Example", "ConvertToDecimal('AB123')"),
             ("Name", "ConvertToDecimal"),
             ("Description", "@|TR|Returns a number converted from string (A=1,...Z=26) in a string|TR|@"),
             ("Parameter", ("Type", "string"), ("Name", "a1"))),
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
      ev.AddFunction("GetRowname", ("Function", ("Result", "String"),
             ("Example", "GetRowname(Coil.LagerortVorLastaufnahme)"),
             ("Name", "GetRowname"),
             ("Description", "@|TR|Gibt den Reihennamen zurück|TR|@"),
             ("Parameter", ("Type", "CStorageLocation"), ("Name", "a1"))),
             o =>
             {
               if (o.Length != 1)
                 throw new WrongNumberOfParameters("~1 Parameter expected in Rowname", 1);
               if (o[0] as CStorageLocation == null)
                 throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
               return (o[0] as CStorageLocation).m_Row;
             });

      ev.AddFunction("GetAreaname", ("Function", ("Result", "String"),
       ("Example", "GetAreaname(Coil.LagerortVorLastaufnahme)"),
       ("Name", "GetAreaname"),
       ("Description", "@|TR|Gibt den Bereichsnamen zurück|TR|@"),
       ("Parameter", ("Type", "CStorageLocation"), ("Name", "a1"))),
       o =>
       {
         if (o.Length != 1)
           throw new WrongNumberOfParameters("~1 Parameter expected in Areaname", 1);
         if (o[0] as CStorageLocation == null)
           throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
         return (o[0] as CStorageLocation).m_Area;
       });

      ev.AddFunction("GetBayname", ("Function", ("Result", "String"),
       ("Example", "GetBayname(Coil.LagerortVorLastaufnahme)"),
       ("Name", "GetBayname"),
       ("Description", "@|TR|Gibt den Hallennamen zurück|TR|@"),
       ("Parameter", ("Type", "CStorageLocation"), ("Name", "a1"))),
       o =>
       {
         if (o.Length != 1)
           throw new WrongNumberOfParameters("~1 Parameter expected in Bayname", 1);
         if (o[0] as CStorageLocation == null)
           throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
         return (o[0] as CStorageLocation).m_Bay;
       });

      ev.AddFunction("GetLocation", ("Function", ("Result", "int"),
        ("Example", "GetLocation(Coil.LagerortVorLastaufnahme)"),
        ("Name", "GetLocation"),
        ("Description", "@|TR|Gibt den Platz zurück|TR|@"),
        ("Parameter", ("Type", "CStorageLocation"), ("Name", "a1"))),
        o =>
        {
          if (o.Length != 1)
            throw new WrongNumberOfParameters("~1 Parameter expected in Rowname", 1);
          if (o[0] as CStorageLocation == null)
            throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
          return (o[0] as CStorageLocation).m_Location;
        });

      ev.AddFunction("GetLayer", ("Function", ("Result", "int"),
        ("Example", "GetLayer(Coil.LagerortVorLastaufnahme)"),
        ("Name", "GetLayer"),
        ("Description", "@|TR|Gibt die Lage zurück|TR|@"),
        ("Parameter", ("Type", "CStorageLocation"), ("Name", "a1"))),
        o =>
        {
          if (o.Length != 1)
            throw new WrongNumberOfParameters("~1 Parameter expected in Rowname", 1);
          if (o[0] as CStorageLocation == null)
            throw new WrongParameterType("~Parameter must be of type CStorageLocation", "a1", typeof(CStorageLocation));
          return (o[0] as CStorageLocation).m_Layer;
        });

      ev.AddFunction("MakeVehicleStorageLocation", ("Function", ("Result", "CStorageLocation"),
         ("Example", "MakeVehicleStorageLocation('UN-C-392, 1')"),
         ("Name", "MakeVehicleStorageLocation"),
         ("Description", "@|TR|Creates a vehicle storage location|TR|@"),
                ("Parameter", ("Type", "String"), ("Name", "v")),
                ("Parameter", ("Type", "String"), ("Name", "t"))),
         o =>
         {
           return new CStorageLocation(CStorageLocation.StorageLocationType.VEHICLE) { m_VehicleID = (string)o[0], m_PositionInVehicle = (string)o[1] };
         });

      ev.AddFunction("ReduceStorageLocationToRow", ("Function", ("Result", "CStorageLocation"),
        ("Example", "ReduceStorageLocationToRow(@|TR|Coil|TR|@.@|TR|StorageLocation|TR|@)"),
        ("Name", "ReduceStorageLocationToRow"),
        ("Description", "@|TR|Reduces a storage location to row detail level|TR|@"),
        ("Parameter", ("Type", "CStorageLocation"), ("Name", "s1"))),
        o =>
        {
          if (o.Length != 1)
            throw new Exception("~1 Parameter expected in ReduceStorageLocationToRow");

          return ((CStorageLocation)o[0]).ReduceToRow();
        });

      AddIsInAttribute(ev);

      ev.AddFunction("IsAtCrane", ("Function", ("Result", "Boolean"),
        ("Example", "IsAtCrane(s1,k1)<br>IsAtCrane(@|TR|Coil|TR|@.@|TR|StorageLocation|TR|@, '18')"),
        ("Name", "IsAtCrane"),
        ("Description", "@|TR|Returns true if s1 is at MOT k1|TR|@"),
        ("Parameter", ("Type", "CStorageLocation"), ("Name", "s1")),
        ("Parameter", ("Type", "String"), ("Name", "k1"))),
          o =>
          {
            if (o.Length != 2)
              throw new Exception("~2 Parameter expected in IsAtCrane");

            if (!((CStorageLocation)o[0]).IsCraneStorage)
              return false;

            return ((CStorageLocation)o[0]).m_Row == (String)o[1];
          });

      ev.AddFunction("GetX", ("Function", ("Result", "Decimal"),
        ("Example", "GetX(c)<br>GetX(@|TR|Coil|TR|@.@|TR|Coordinates|TR|@)"),
        ("Name", "GetX"),
        ("Description", "@|TR|Returns the x value of a coordinate|TR|@"),
        ("Parameter", ("Type", "CCoord3D"), ("Name", "c"))),
          o => (decimal)((CCoord3D)o[0]).m_X);

      ev.AddFunction("GetY", ("Function", ("Result", "Decimal"),
        ("Example", "GetY(c)<br>GetY(@|TR|Coil|TR|@.@|TR|Coordinates|TR|@)"),
        ("Name", "GetY"),
        ("Description", "@|TR|Returns the y value of a coordinate|TR|@"),
        ("Parameter", ("Type", "CCoord3D"), ("Name", "c"))),
          o => (decimal)((CCoord3D)o[0]).m_Y);

      ev.AddFunction("GetZ", ("Function", ("Result", "Decimal"),
        ("Example", "GetZ(c)<br>GetZ(@|TR|Coil|TR|@.@|TR|Coordinates|TR|@)"),
        ("Name", "GetZ"),
        ("Description", "@|TR|Returns the z value of a coordinate|TR|@"),
        ("Parameter", ("Type", "CCoord3D"), ("Name", "c"))),
          o => (decimal)((CCoord3D)o[0]).m_Z);

      ev.AddFunction("ParseInt", ("Function", ("Result", "Decimal"),
        ("Example", "ParseInt(c)<br>ParseInt('123')"),
        ("Name", "ParseInt"),
        ("Description", "@|TR|Interprets the given string as an integer|TR|@"),
        ("Parameter", ("Type", "String"), ("Name", "c"))),
          o =>
          {
            var f = (String)o[0];
            int i;
            int.TryParse(f, out i);
            return (decimal)i;
          });

      ev.AddFunction("Round", ("Function",
        ("Example", "Round(123.7) => 123"),
        ("Result", "Decimal"),
        ("Name", "Round"),
        ("Description", "Rounds the given decimal"),
        ("Parameter", ("Type", "decimal"), ("Name", "p1"))),
        o =>
        {
          if (o.Length == 1)
            return decimal.Round((decimal)o[0]);
          return decimal.Round((decimal)o[0], (int)(decimal)o[1]);
        });

      ev.AddFunction("YesNo", ("Function",
        ("Example", "YesNo(true)) => 'Y'"),
        ("Result", "String"),
        ("Name", "YesNo"),
        ("Description", "Converts the given bool to Yes/No"),
        ("Parameter", ("Type", "Boolean"), ("Name", "p1"))),
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
                        ("Function", ("Result", "Boolean"),
                        ("Example", "IsOnShortTermSequence('HSM')"),
                        ("Name", "IsOnShortTermSequence"),
                        ("Description", "Returns true, if the material is on the short term sequence of the specified unit."),
                        ("Parameter", ("Type", "String"), ("Name", "unit"))), delegate (Object[] o)
                        {
                          if (o.Length != 1)
                            throw new Exception("~1 Parameter expected in IsOnShortTermSequence");

                          if (del != null)
                            return del(o[0] as string);

                          return false;
                        });


      ev.AddFunction("IsOnLongTermSequence",
                       ("Function", ("Result", "Boolean"),
                       ("Example", "IsOnLongTermSequence('HSM')"),
                       ("Name", "IsOnLongTermSequence"),
                       ("Description", "Returns true, if the material is on the long term sequence of the specified unit."),
                       ("Parameter", ("Type", "String"), ("Name", "unit"))), delegate (Object[] o)
                       {
                         if (o.Length != 1)
                           throw new Exception("~1 Parameter expected in IsOnLongTermSequence");

                         if (del != null)
                           return del(o[0] as string);

                         return false;
                       });
    }

      ev.AddFunction("IsInAttribute", ("Function", ("Result", "Boolean"),
        ("Example", "IsInAttribute(s1,a1)<br>IsInAttribute(@|TR|Coil|TR|@.@|TR|StorageLocation|TR|@, '@|TR|Main storage|TR|@')"),
        ("Name", "IsInAttribute"),
        ("Description", "@|TR|Returns true if s1 is located within attribute a1|TR|@"),
        ("Parameter", ("Type", "CStorageLocation"), ("Name", "s1")),
        ("Parameter", ("Type", "String"), ("Name", "a1"))),
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
    public static void AddEvaluatorDummies(Evaluator ev)
    {


      ev.AddFunction("GetCoilUtilization", ("Function", ("Result", "Decimal"),
        ("Example", "GetCoilUtilization(b)<br>GetCoilUtilization('21PP')"),
        ("Name", "GetCoilUtilization"),
        ("Description", "@|TR|Returns coil utilization in statistic group b|TR|@"),
        ("Parameter", ("Type", "String"), ("Name", "b"))),
          o =>
          {
            if (o.Length != 1)
              throw new Exception("~1 Parameter expected in GetCoilUtilization");

            return 0;
          });
    }
  }

ev.AddFunction("Count", ("Function",
	("Result", "Decimal"),
	("Example", "Count('@|TR|Coil|TR|@.@|TR|Width|TR|@ > 1000') > 2)"),
	("Name", "Count"),
	("Description", "@|TR|Returns the number of true conditions for a list|TR|@"),
	("Parameter", ("Type", "String"), ("Name", "a1"))),
	(o) =>
	{
		if (o.Length != 1)
			throw new Exception("Only one parameter allowed!");
		String a1 = (String)o[0];
		int count = 0;
		var expr = ev.GetExpression(a1);
		var Candidates = (List<CCoil>)ev.GetObject(prefix + ".Coils");
		foreach (var item in Candidates)
		{
			CWob.AddWobParameters(item, ev);
			if ((bool)expr.Evaluate(ev))
				count++;
		}
		return count;
	});
}

    public static void AddShortTermSequenceFunctionality(Evaluator ev, AddIsOnShortTermSequence del1, AddGetShortTermSequencePosition del2)
    {
      ev.AddFunction("IsOnShortTermSequence", ("Function",
          ("Example", "IsOnShortTermSequence(Coil.Ident)"),
          ("Result", "Boolean"),
          ("Name", "IsOnShortTermSequence"),
          ("Description", "@|TR|Returns whether a coil is on a given short term sequence|TR|@"),
          ("Parameter", ("Type", "string"), ("Name", "ident")),
          ("Parameter", ("Type", "string"), ("Name", "unit"))),
      delegate (Object[] o)
      {
        var c1 = (string)o[0];
        var c2 = (string)o[1];
        if (del1 != null)
          return del1(c1,c2);
        return false;
      });

      ev.AddFunction("GetShortTermSequencePosition", ("Function",
          ("Example", "GetShortTermSequencePosition(Coil.Ident)"),
          ("Result", "decimal"),
          ("Name", "GetShortTermSequencePosition"),
          ("Description", "@|TR|Returns the position of a coil if it is on a given short term sequence - or 999 if it is not on the given sequence|TR|@"),
          ("Parameter", ("Type", "string"), ("Name", "ident")),
          ("Parameter", ("Type", "string"), ("Name", "unit"))),
      delegate (Object[] o)
      {
        var c1 = (string)o[0];
        var c2 = (string)o[1];
        if (del2 != null)
          return del2(c1, c2);
        return 999m;
      });
    }
      ev.AddFunction("GetShortTermSequencePosition", ("Function",
          ("Example", "GetShortTermSequencePosition(Coil.Ident)"),
          ("Result", "decimal"),
          ("Name", "GetShortTermSequencePosition"),
          ("Description", "@|TR|Returns the position of a coil if it is on a given short term sequence - or 999 if it is not on the given sequence|TR|@"),
          ("Parameter", ("Type", "string"), ("Name", "ident")),
          ("Parameter", ("Type", "string"), ("Name", "unit"))),
      delegate (Object[] o)
      {
        var c1 = (string)o[0];
        var c2 = (string)o[1];
        if (del2 != null)
          return del2(c1, c2);
        return 999m;
      });
    }
