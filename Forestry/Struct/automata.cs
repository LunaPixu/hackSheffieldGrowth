
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Drawing;

namespace Automata;

/// <summary>
/// One cell as part of an automata meant to simulate some digital forestry.
/// </summary>
/// <param name="type">Type to default the cell to.</param>
/// <param name="age">Current age of cell (default 0)</param>
partial struct Cell(int type, int age = 0)
{
	private int _type = type; // Current state (type) of this cell.
	public int Type { readonly get {return _type;} set { Lifespan = 0; _type=value; }}
	public int Lifespan = age; // Amount of iterations that have passed without the type of the cell changing.

	// Static random number generation used by this struct
	static readonly Random r = new(); // bad practice hacky ultramarine powertool ventricule explosion

	public override readonly int GetHashCode()
	{
		return Type;
	}

	public override readonly bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj == null) return false;
		return GetHashCode() == obj.GetHashCode();
	}

	public readonly bool Equals(Cell cell)
	{
		return Type == cell.Type;
	}

	/// <summary>
	/// Method returning the resultant cell after having reacted to the adjacent ring of cells.
	/// </summary>
	/// <param name="Ring">Ring of cells adjacent to this one.</param>
	/// <returns>Reactionary cell.</returns>
	public Cell React(Cell[] Ring) {
		Lifespan++;
		int shade = 0;
		foreach (Cell v in Ring) {
			if (Type > 0 && v.Type == -1) {
				return new(0);
			}
			if (v.Type == 4) {
				// There is a tall (type 4) cell nearby, casting shade.
				shade ++;
			}
		}
		if (Type == 0) {
			if (shade > 0) return r.NextDouble() > .05 ? this : new(1);
			else return this;
		}
		else if (Type == -1) {
			if (shade <= 2) return new(2);
			else return Next;
		}
		else if (shade >= 7 || Lifespan >= 4) return new(-1); // Old/too shaded cells will die, becoming type -1.
		else if (shade >= 3) return this; // Moderately shaded 
		else return Next;
	}

	/// <summary>
	/// Specialty method to convert these particular cells to characters (used in CellGrid.ToString())
	/// </summary>
	/// <returns>Character representing state of this cell.</returns>
	public readonly char ToChar()
	{
		// ⊢⊣⊤⊥
		switch (Type)
		{
			case 0: return ' ';
			case 1: return '.';
			case 2: return 'o';
			case 3: return 'O';
			case 4: return '@';
			case -1: return 'X';
			default: return ' ';
		}
	}

	/// <summary>
	/// Alternate ToChar retuning block symbols.
	/// </summary>
	/// <returns>Character representing state of this cell.</returns>
	public readonly char ToBlock() {
		switch (Type) {
			case 1: return '░';
			case 2: return '▒';
			case 3: return '▓';
			case 4: return '█';
			default: return ' ';
		}
	}


	readonly public Cell Next {get {
		// Returns the next logical step in the cell cycle.
		switch (Type) {
			case 0: return new(0);
			case 1: return new(2);
			case 2: return new(3);
			case 3: return new(4);
			case 4: return new(4);
			case -1: return new(-1);
			default: return new(0);
		}
	} }

	public static Cell Empty = new(0);
}

/// <summary>
/// Array of cells (CellGrid.Grid) with additional helper functions facilitating automata passes on the data.
/// </summary>
partial class CellGrid
{
	/// <summary>
	/// Internal data of the cell grid.
	/// </summary>
	public Cell[,] Grid;
	readonly int _sizx;
	readonly int _sizy;
	public (int x, int y) Size { get { return (_sizx, _sizy); } }

	public CellGrid((int x, int y) size)
	{
		(_sizx, _sizy) = size;
		Grid = new Cell[size.x, size.y];
	}

	public override string ToString()
	{
		return ToText();
	}

	/// <summary>
	/// Convert the cells in the grid to a textual representation (prints nicely on console)
	/// </summary>
	/// <param name="ToChar">Parser to use to convert each cell to a character.</param>
	/// <returns>Stringified grid.</returns>
	public string ToText( Func<Cell, char> ToChar ) {
		string o = "";
		for (int i = 0; i < Size.x; i++)
		{
			for (int j = 0; j < Size.y; j++)
			{
				o += ToChar(Grid[i, j]);
			}
			o += "\n";
		}
		return o;
	}

	// this line looks awful but perfectly describes what is being done
	public string ToText() => ToText((v) => { return v.ToChar(); });

	public override int GetHashCode()
	{
		var o = new System.HashCode();
		o.Add(_sizx);
		o.Add(_sizy);
		foreach (var v in Grid) {
			o.Add(v);
		}
		return o.ToHashCode();
	}

	/// <summary>
	/// Collects adjacent cells to this position in the form of an array.
	/// Out of bounds will be represented by Cell.Empty.
	/// </summary>
	/// <param name="i">The i (aka x) coordinate to fetch the adjacency from.</param>
	/// <param name="j">The j (aka y) coordinate to fetch the adjacency from.</param>
	/// <returns>
	/// Array of cells in clockwise arrangment from north:
	/// [N,NE,E,SE,S,SW,W,NW]
	/// </returns>
	public Cell[] Adjacency(int i, int j)
	{
		Cell[] o = new Cell[8];
		(int x, int y)[] positions = [ (0,1), (1,1), (1,0), (1,-1), (0,-1), (-1,-1), (-1,0), (-1,1) ];
		for (int k = 0; k < 8; k++) {
			if (
				i-positions[k].x >= 0 && i-positions[k].x < Size.x &&
				j-positions[k].y >= 0 && j-positions[k].y < Size.y
				)
				o[k] = Grid[i-positions[k].x, j-positions[k].y];
			else o[k] = Cell.Empty;
		}
		return o;
	}

	/// <summary>
	/// Run one pass of the cell's automata on this data. Operates upon the data contained itself.
	/// </summary>
	public void Automata()
	{
		Cell[,] buffer = (Cell[,])Grid.Clone();
		for (int i = 0; i < Size.x; i++)
		{
			for (int j = 0; j < Size.y; j++)
			{
				Cell v = Grid[i, j];
				var adjacency = Adjacency(i, j);
				buffer[i,j] = v.React(adjacency);
			}
		}
		// Apply the changes
		Grid = buffer;
	}
}
