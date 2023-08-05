using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Globalization;
using System.Xml;

if( args.Length < 3)
{
    Console.Error.WriteLine("Error: No has especificado los parámetros necesarios");
    Console.Error.WriteLine("ImportarCodigosCsvTelespazzio [archivo csv] [tabla de códigos a la que añadir códigos] [archivo a crear]");
    return;
}
var registros = CargaCsv(args[0]);

XmlDocument digiTab = new XmlDocument();
digiTab.Load(args[1]);

XmlNamespaceManager ns = new XmlNamespaceManager(digiTab.NameTable);
ns.AddNamespace("iodgn", "http://schemas.digi21.net/DigiNG/IO/dgnv8/v1.0");
var root = digiTab.DocumentElement;
var codigos = root.SelectSingleNode("/digitab/codes");
foreach (var record in registros)
{
    var caracteristicas = new Dictionary<string, string>();
    foreach(var característica in record.AltresCaracterístiques.Split())
    {
        if (string.IsNullOrEmpty(característica.Trim()))
            continue;

        var palabras = característica.Split('=');
        if (palabras.Length == 2)
        {
            caracteristicas[palabras[0]] = palabras[1];

            while (caracteristicas[palabras[0]].Contains("“"))
                caracteristicas[palabras[0]] = caracteristicas[palabras[0]].Replace("“", string.Empty);

            while (caracteristicas[palabras[0]].Contains("”"))
                caracteristicas[palabras[0]] = caracteristicas[palabras[0]].Replace("”", string.Empty);
        }
    }

    Console.WriteLine(record.Concepte);

    var codigo = digiTab.CreateNode(XmlNodeType.Element, "code", "");
    AnadeAtributo(digiTab, codigo, "name", record.Concepte);
    AnadeAtributo(digiTab, codigo, "description", record.Concepte);
    AnadeAtributo(digiTab, codigo, "priority", "0");
    AnadeAtributo(digiTab, codigo, "tags", string.Empty);
    AnadeAtributo(digiTab, codigo, "alias", string.Empty);
    AnadeAtributo(digiTab, codigo, "type", record.ElementType == "Cell" ? "0" : "1");
    AnadeAtributo(digiTab, codigo, "helpFile", string.Empty);
    AnadeAtributo(digiTab, codigo, "table", string.Empty);
    AnadeAtributo(digiTab, codigo, "conditions", string.Empty);
    AnadeAtributo(digiTab, codigo, "enabled", "1");
    AnadeAtributo(digiTab, codigo, "print", "1");
    AnadeAtributo(digiTab, codigo, "streamMode", "0");
    AnadeAtributo(digiTab, codigo, "applySemanticModel", "0");
    AnadeAtributo(digiTab, codigo, "auto", "1");
    AnadeAtributo(digiTab, codigo, "sizeInPixel", "0");
    codigos.AppendChild(codigo);

    var representacion = digiTab.CreateNode(XmlNodeType.Element, "representation", "");
    AnadeAtributo(digiTab, representacion, "style", string.Empty);
    AnadeAtributo(digiTab, representacion, "color", record.Color);
    AnadeAtributo(digiTab, representacion, "color-stereo", record.Color);
    AnadeAtributo(digiTab, representacion, "width", record.Weight);
    AnadeAtributo(digiTab, representacion, "width-stereo", record.Weight);
    AnadeAtributo(digiTab, representacion, "print-color", "0");
    AnadeAtributo(digiTab, representacion, "print-width", "0.01");
    AnadeAtributo(digiTab, representacion, "fontTT", caracteristicas.ContainsKey("Font") ? caracteristicas["Font"] : "0");
    AnadeAtributo(digiTab, representacion, "charSet", "0");
    AnadeAtributo(digiTab, representacion, "weightTT", "0");
    AnadeAtributo(digiTab, representacion, "italic", "0");
    codigo.AppendChild(representacion);

    var io = digiTab.CreateNode(XmlNodeType.Element, "io", "");
    codigo.AppendChild(io);

    var dgn = digiTab.CreateNode(XmlNodeType.Element, "iodgn:transform", "http://schemas.digi21.net/DigiNG/IO/dgnv8/v1.0");
    AnadeAtributo(digiTab, dgn, "level", record.LevelName);
    AnadeAtributo(digiTab, dgn, "colorEntity", record.Color);
    AnadeAtributo(digiTab, dgn, "styleEntity", record.LineStyle);
    AnadeAtributo(digiTab, dgn, "weightEntity", record.Weight);
    AnadeAtributo(digiTab, dgn, "fillColor", "-1");
    AnadeAtributo(digiTab, dgn, "gg", "0");
    AnadeAtributo(digiTab, dgn, "class", record.Class);
    AnadeAtributo(digiTab, dgn, "color", record.Color);
    AnadeAtributo(digiTab, dgn, "style", record.LineStyle);
    AnadeAtributo(digiTab, dgn, "weight", record.Weight);
    AnadeAtributo(digiTab, dgn, "viewIndependent", "0");
    AnadeAtributo(digiTab, dgn, "notSnappable", "0");
    AnadeAtributo(digiTab, dgn, "useThTw", "0");
    AnadeAtributo(digiTab, dgn, "th", "1.0");
    AnadeAtributo(digiTab, dgn, "tw", "1.0");
    AnadeAtributo(digiTab, dgn, "tagSetName", string.Empty);
    io.AppendChild(dgn);

    var point = digiTab.CreateNode(XmlNodeType.Element, "iodgn:point", "http://schemas.digi21.net/DigiNG/IO/dgnv8/v1.0");
    AnadeAtributo(digiTab, point, "type", caracteristicas.ContainsKey("Cell") ? "CELL" : "POINT");
    AnadeAtributo(digiTab, point, "cell", caracteristicas.ContainsKey("Cell") ? caracteristicas["Cell"] : string.Empty);
    AnadeAtributo(digiTab, point, "sx", "1.0");
    AnadeAtributo(digiTab, point, "sy", "1.0");
    AnadeAtributo(digiTab, point, "sz", "1.0");

    dgn.AppendChild(point);

    var line = digiTab.CreateNode(XmlNodeType.Element, "iodgn:line", "http://schemas.digi21.net/DigiNG/IO/dgnv8/v1.0");
    AnadeAtributo(digiTab, line, "type", "LINESTRING");
    dgn.AppendChild(line);

    var text = digiTab.CreateNode(XmlNodeType.Element, "iodgn:text", "http://schemas.digi21.net/DigiNG/IO/dgnv8/v1.0");
    AnadeAtributo(digiTab, text, "font", caracteristicas.ContainsKey("Font") ? caracteristicas["Font"] : "1");
    dgn.AppendChild(text);
}

digiTab.Save(args[2]);

static List<CamposCsv> CargaCsv(string ruta)
{
    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        Delimiter = ";"
    };

    using (var reader = new StreamReader(ruta))
    using (var csv = new CsvReader(reader, config))
    {
        return csv.GetRecords<CamposCsv>().ToList();
    }
}

static void AnadeAtributo(XmlDocument digiTab, XmlNode codigo, string atributo, string valor)
{
    var nombre = (XmlAttribute)digiTab.CreateNode(XmlNodeType.Attribute, atributo, "");
    nombre.InnerText = valor;
    codigo.Attributes.Append(nombre);
}

public class CamposCsv
{
    [Index(0)]
    public string Concepte { get; set; }
    [Index(1)]
    public string ElementType { get; set; }
    [Index(2)]
    public string Level { get; set; }
    [Index(3)]
    public string Color { get; set; }
    [Index(4)]
    public string LineStyle { get; set; }
    [Index(5)]
    public string Weight { get; set; }
    [Index(6)]
    public string Class { get; set; }
    [Index(7)]
    public string AltresCaracterístiques { get; set; }
    [Index(8)]
    public string LevelName { get; set; }
    [Index(9)]
    public string Representacio { get; set; }
};

