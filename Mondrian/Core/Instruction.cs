﻿namespace Core
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
    }

    public class NopInstruction : Instruction {
        public NopInstruction()
        {
            Typ = InstructionType.Nop;
        }
    };

    public class CommentInstruction : Instruction {
        public string Comment { get; private set; }
        public CommentInstruction(string comment)
        {
            Typ = InstructionType.Comment;
            Comment = comment;
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
    };

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
    };

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
    };

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
    };
}
