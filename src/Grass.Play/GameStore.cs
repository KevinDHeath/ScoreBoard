// Ignore Spelling: json
using System.Text.Json;
using Grass.Logic.Models;
namespace Grass.Play;

public class GameStore
{
	public List<Summary> Games { get; set; } = [];

	private readonly JsonSerializerOptions options = new() { WriteIndented = false };

	public void StoreSummary( Game game )
	{
		Games.Add( Summary.BuildSummary( game ) );
	}

	public string ExportSummary( Summary summary, bool indent = false )
	{
		options.WriteIndented = indent;
		return JsonSerializer.Serialize( summary, options );
	}

	public Summary? ImportSummary( ref string json )
	{
		try { return JsonSerializer.Deserialize<Summary>( json ); }
		catch { return null; }
	}
}