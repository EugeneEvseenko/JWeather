using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWeather
{
        
public class Rootobject
{
public Coord coord { get; set; }
public Weather[] weather { get; set; }
public string _base { get; set; }
public Main main { get; set; }
public double visibility { get; set; }
public Wind wind { get; set; }
public Clouds clouds { get; set; }
public Rain rain { get; set; }
public Snow snow { get; set; }
public double dt { get; set; }
public Sys sys { get; set; }
public int id { get; set; }
public string name { get; set; }
public int cod { get; set; }
}

public class Coord
{
public float lon { get; set; }
public float lat { get; set; }
}

public class Main
{
public double temp { get; set; }
public double pressure { get; set; }
public int humidity { get; set; }
public double temp_min { get; set; }
public double temp_max { get; set; }
}

public class Wind
{
public double speed { get; set; }
public double deg { get; set; }
}

public class Clouds
{
public int all { get; set; }
}
public class Rain
{
    public double h { get; set; }
}
public class Snow
{
    public double h { get; set; }
}

public class Sys
{
public int type { get; set; }
public int id { get; set; }
public float message { get; set; }
public string country { get; set; }
public int sunrise { get; set; }
public int sunset { get; set; }
}

public class Weather
{
public int id { get; set; }
public string main { get; set; }
public string description { get; set; }
public string icon { get; set; }
}

    
}
