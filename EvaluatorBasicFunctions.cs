public void AddBasicFunctions()
  {
    AddBasicObject("true", true);
    AddBasicObject("false", false);
    AddBasicObject("ALWAYS", true);
    AddBasicObject("PI", (decimal)Math.PI);
    
    

    AddFunction("ToString", new XElement("Function",
      new XAttribute("Example", "ToString(p1)<br>ToString(123) => '123'"),
      new XAttribute("Result", "String"),
      new XAttribute("Name", "ToString"),
      new XAttribute("Description", "@|TR|Converts the given object to String|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "Object"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          return o[0].ToString();
        });

    AddFunction("TimeSpan.ToSeconds", new XElement("Function",
      new XAttribute("Example", "TimeSpan.ToSeconds(p1)"),
      new XAttribute("Result", "Decimal"),
      new XAttribute("Name", "TimeSpan.ToSeconds"),
      new XAttribute("Description", "@|TR|Returns the time difference in seconds|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "TimeSpan"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          return (decimal)((TimeSpan)o[0]).TotalSeconds;
        });

    AddFunction("TimeSpan.ToMinutes", new XElement("Function",
      new XAttribute("Example", "TimeSpan.ToMinutes(p1)"),
      new XAttribute("Result", "Decimal"),
      new XAttribute("Name", "TimeSpan.ToMinutes"),
      new XAttribute("Description", "@|TR|Returns the time difference in minutes|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "TimeSpan"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          return (decimal)((TimeSpan)o[0]).TotalMinutes;
        });

    AddFunction("TimeSpan.ToHours", new XElement("Function",
      new XAttribute("Example", "TimeSpan.ToHours(p1)"),
      new XAttribute("Result", "Decimal"),
      new XAttribute("Name", "TimeSpan.ToHours"),
      new XAttribute("Description", "@|TR|Returns the time difference in hours|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "TimeSpan"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          return (decimal)((TimeSpan)o[0]).TotalHours;
        });

    AddFunction("TimeSpan.ToDays", new XElement("Function",
      new XAttribute("Example", "TimeSpan.ToDays(p1)"),
      new XAttribute("Result", "Decimal"),
      new XAttribute("Name", "TimeSpan.ToDays"),
      new XAttribute("Description", "@|TR|Returns the time difference in days|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "TimeSpan"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          return (decimal)((TimeSpan)o[0]).TotalDays;
        });

    AddFunction("DateTime.Now", new XElement("Function",
      new XAttribute("Example", "DateTime.Now() => DateTime Object"),
      new XAttribute("Result", "DateTime"),
      new XAttribute("Name", "DateTime.Now"),
      new XAttribute("Description", "@|TR|Returns the current date/time|TR|@")),
        delegate (Object[] o)
        {
          return DateTime.Now;
        });

    AddFunction("DateTime.Parse", new XElement("Function",
      new XAttribute("Example", "DateTime.Parse(p1)<br>DateTime.Parse('11.5.2009') => DateTime Object"),
      new XAttribute("Result", "DateTime"),
      new XAttribute("Name", "DateTime.Parse"),
      new XAttribute("Description", "@|TR|Parses a string for a date/time.|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          return DateTime.Parse(o[0].ToString());
        });

    AddFunction("HasValue", new XElement("Function",
      new XAttribute("Example", "HasValue(p1)<br>HasValue('06.12.2013') => True"),
      new XAttribute("Result", "Boolean"),
      new XAttribute("Name", "HasValue"),
      new XAttribute("Description", "@|TR|Returns true if p1 has a date/time value.|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "DateTime"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          DateTime d = (DateTime)o[0];
          return d > DateTime.MinValue;
        });

    AddFunction("ToDecimal", new XElement("Function",
      new XAttribute("Example", "ToDecimal(p1)<br>ToDecimal('123') => 123"),
      new XAttribute("Result", "Decimal"),
      new XAttribute("Name", "ToDecimal"),
      new XAttribute("Description", "@|TR|Converts the given string to a decimal if possible|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          decimal result = 0;
          decimal.TryParse((String)o[0], System.Globalization.NumberStyles.None | System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out result);
          return result;
        });

    AddFunction("Abs", new XElement("Function",
      new XAttribute("Example", "Abs(p1)<br>Abs(-7) => 7"),
      new XAttribute("Result", "Decimal"),
      new XAttribute("Name", "Abs"),
      new XAttribute("Description", "@|TR|Returns the absolute value of a decimal value|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "Decimal"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          decimal v = Evaluator.Expr.DecimalCast(o[0]);
          return Math.Abs(v);
        });

    AddFunction("Sqrt", new XElement("Function",
      new XAttribute("Example", "Sqrt(p1)<br>Sqrt(64) => 8"),
      new XAttribute("Result", "Decimal"),
      new XAttribute("Name", "Sqrt"),
      new XAttribute("Description", "@|TR|Returns the square root|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "Decimal"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          decimal v = Evaluator.Expr.DecimalCast(o[0]);
          if (v < 0) return 0;
          return (decimal)Math.Sqrt((double)v);
        });

    AddFunction("Pow", new XElement("Function",
      new XAttribute("Example", "Pow(p1)<br>Pow(8) => 64"),
      new XAttribute("Result", "Decimal"),
      new XAttribute("Name", "Pow"),
      new XAttribute("Description", "@|TR|Squares the given digit|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "Decimal"), new XAttribute("Name", "p1"))),
        delegate (Object[] o)
        {
          decimal v = Evaluator.Expr.DecimalCast(o[0]);
          return (decimal)(v * v);
        });

    AddFunction("SubStr", new XElement("Function",
      new XAttribute("Example", "SubStr(s,@|TR|StartIdx|TR|@[,@|TR|StringLength|TR|@])<br>SubStr('ABC',1) => 'BC'<br>SubStr('ABC',0,1) => 'A'"),
      new XAttribute("Result", "String"),
      new XAttribute("Name", "SubStr"),
      new XAttribute("Description", "@|TR|Returns a substring from a string.|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "s")),
      new XElement("Parameter", new XAttribute("Type", "Decimal"), new XAttribute("Name", "@|TR|StartIdx|TR|@")),
      new XElement("Parameter", new XAttribute("Type", "Decimal"), new XAttribute("Name", "[@|TR|StringLength|TR|@]"))),
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

    AddFunction("Contains", new XElement("Function",
      new XAttribute("Example", "Contains(s1,s2)<br>Contains('ABCDE','BCD') => True"),
      new XAttribute("Result", "Boolean"),
      new XAttribute("Name", "Contains"),
      new XAttribute("Description", "@|TR|Returns true if s1 contains s2.|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "s1")),
      new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "s2"))),
        delegate (Object[] o)
        {
          String s1 = (String)o[0];
          String s2 = (String)o[1];
          return s1.Contains(s2);
        });

    AddFunction("StartsWith", new XElement("Function",
      new XAttribute("Example", "StartsWith(s1,s2)<br>StartsWith('ABCDE','AB') => True"),
      new XAttribute("Result", "Boolean"),
      new XAttribute("Name", "StartsWith"),
      new XAttribute("Description", "@|TR|Returns true if s1 starts with s2.|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "s1")),
      new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "s2"))),
        delegate (Object[] o)
        {
          String s1 = (String)o[0];
          String s2 = (String)o[1];
          return s1.StartsWith(s2);
        });

    AddFunction("EndsWith", new XElement("Function",
      new XAttribute("Example", "EndsWith(s1,s2)<br>EndsWith('ABCDE','DE') => True"),
      new XAttribute("Result", "Boolean"),
      new XAttribute("Name", "EndsWith"),
      new XAttribute("Description", "@|TR|Returns true if s1 ends with s2.|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "s1")),
      new XElement("Parameter", new XAttribute("Type", "String"), new XAttribute("Name", "s2"))),
        delegate (Object[] o)
        {
          String s1 = (String)o[0];
          String s2 = (String)o[1];
          return s1.EndsWith(s2);
        });

    AddFunction("In", new XElement("Function",
      new XAttribute("Example", "In(p1,p2,...)<br>In(1,3,5,7,1) => True<br>In(2,3,5,7,1) => False <br>"),
      new XAttribute("Result", "Boolean"),
      new XAttribute("Name", "In"),
      new XAttribute("Description", "@|TR|Searches a list of strings/int (p2,...) for p1.|TR|@"),
      new XElement("Parameter", new XAttribute("Type", "String/Decimal"), new XAttribute("Name", "p1")),
      new XElement("Parameter", new XAttribute("Type", "String/Decimal"), new XAttribute("Name", "p2")),
      new XElement("Parameter", new XAttribute("Type", "String/Decimal"), new XAttribute("Name", "..."))),
        delegate (Object[] o)
        {
          for (int i = 1; i < o.Length; i++)
          {
            if (o[0].Equals(o[i]))
              return true;
          }
          return false;
        });

		AddFunction("CalendarWeek", new XElement("Function",
			new XAttribute("Example", String.Format("CalendarWeek() => {0}", GetCalenderWeek(DateTime.Now))),
			new XAttribute("Result", "Decimal"),
			new XAttribute("Name", "CalendarWeek"),
			new XAttribute("Description", "@|TR|Returns the current calendar week|TR|@")),
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