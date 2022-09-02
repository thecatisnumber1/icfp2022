namespace Core
{
    public enum InstructionType
    {
        Nop,
        Comment,
        Color,
        PointCut,
        VerticalCut,
        HorizontalCut,
        Swap,
        Merge
    }

    public abstract class Instruction
    {
        public InstructionType Typ { get; protected set; }
        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }

    public class NopInstruction : Instruction {
        public NopInstruction()
        {
            Typ = InstructionType.Nop;
        }

        public override string ToString()
        {
            return "Nop";
        }
    }

    public class CommentInstruction : Instruction {
        public string Comment { get; private set; }
        public CommentInstruction(string comment)
        {
            Typ = InstructionType.Comment;
            Comment = comment;
        }

        public override string ToString()
        {
            return $"# {Comment}";
        }
    }

    public class ColorInstruction : Instruction
    {
        public string BlockId { get; private set; }
        public RGBA Color { get; private set; }
        public ColorInstruction(string blockId, RGBA color)
        {
            Typ = InstructionType.Color;
            BlockId = blockId;
            Color = color;
        }

        public override string ToString()
        {
            return $"color [{BlockId}] {Color}";
        }
    }
    public class PointCutInstruction : Instruction
    {
        public string BlockId { get; private set; }
        public Point Point { get; private set; }
        public PointCutInstruction(string blockId, Point point)
        {
            Typ = InstructionType.PointCut;
            BlockId = blockId;
            Point = point;
        }

        public override string ToString()
        {
            return $"cut [{BlockId}] {Point}";
        }
    }
    public class VerticalCutInstruction : Instruction
    {
        public string BlockId { get; private set; }
        public int LineNumber { get; private set; }

        public VerticalCutInstruction(string blockId, int lineNumber)
        {
            Typ = InstructionType.VerticalCut;
            BlockId = blockId;
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            return $"cut [{BlockId}] x [{LineNumber}]";
        }
    }
    public class HorizontalCutInstruction : Instruction
    {
        public string BlockId { get; private set; }
        public int LineNumber { get; private set; }

        public HorizontalCutInstruction(string blockId, int lineNumber)
        {
            Typ = InstructionType.HorizontalCut;
            BlockId = blockId;
            LineNumber = lineNumber;
        }

        public override string ToString()
        {
            return $"cut [{BlockId}] y [{LineNumber}]";
        }
    };

    public class SwapInstruction : Instruction
    {
        public string BlockId1 { get; private set; }
        public string BlockId2 { get; private set; }
        public SwapInstruction(string blockId1, string blockId2)
        {
            Typ = InstructionType.Swap;
            BlockId1 = blockId1;
            BlockId2 = blockId2;
        }

        public override string ToString()
        {
            return $"swap [{BlockId1}] [{BlockId2}]";
        }
    };

    public class MergeInstruction : Instruction
    {
        public string BlockId1 { get; private set; }
        public string BlockId2 { get; private set; }
        public MergeInstruction(string blockId1, string blockId2)
        {
            Typ = InstructionType.Merge;
            BlockId1 = blockId1;
            BlockId2 = blockId2;
        }

        public override string ToString()
        {
            return $"merge [{BlockId1}] [{BlockId2}]";
        }
    };
}
