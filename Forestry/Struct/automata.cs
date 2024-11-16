
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Drawing;

namespace Automata;

struct Cell(int type)
{
	public int Type = type;

	static Random r = new(); // bad practice hacky ultra power explosion
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

	public readonly Cell React(Cell[] Ring) {

		int shade = 0;
		foreach (Cell v in Ring) {
			/* if (Type > 0 && v.Type == -1) {
				return new(0);
			} */
			if (v.Type == 4) {
				shade ++;
			}
		}
		if (Type == 0) {
			if (shade > 0) return r.NextDouble() > .9 ? this : new(1);
			else return this;
		}
		if (Type == -1) return Next;
		else if (shade >= 7) return new(-1);
		else if (shade >= 3) return this;
		else return Next;
	}

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

class CellGrid
{
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

	public string ToText() {
		return ToText((v) => { return v.ToChar(); });
	}

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

	public Cell[] Adjacency(int i, int j)
	{
		Cell[] o = new Cell[8];
		(int x, int y)[] positions = [ (0,1), (1,1), (1,0), (1,-1), (0,-1), (-1,-1), (-1,0), (-1,1) ];
		for (int k = 0; k < 8; k++) {
			if (
				i+positions[k].x >= 0 && i+positions[k].x < Size.x &&
				j+positions[k].y >= 0 && j+positions[k].y < Size.y
				)
				o[k] = Grid[i+positions[k].x, j+positions[k].y];
			else o[k] = new(0);
		}
		return o;
	}

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
