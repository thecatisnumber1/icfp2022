using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace AI
{
    public abstract class RectMutation
    {
        public abstract bool CanMutate();
        public abstract Rectangle Mutate();

        public static List<RectMutation> AllMutations(int amount, Rectangle rect)
        {
            return new List<RectMutation>
                {
                    new TopMutation(amount, rect),
                    new TopMutation(-amount, rect),
                    new BottomMutation(amount, rect),
                    new BottomMutation(-amount, rect),
                    new RightMutation(amount, rect),
                    new RightMutation(-amount, rect),
                    new LeftMutation(amount, rect),
                    new LeftMutation(-amount, rect),
                };
        }
    }

    class TopMutation : RectMutation
    {
        private int amount;
        private Rectangle rect;

        public TopMutation(int amount, Rectangle rect)
        {
            this.amount = amount;
            this.rect = rect;
        }

        public override Rectangle Mutate()
        {
            return new Rectangle(rect.BottomLeft, new Point(rect.Right, rect.Top + amount));
        }

        public override bool CanMutate()
        {
            return rect.Top + amount > 0 && rect.Top + amount <= 400 && rect.Top + amount > rect.Bottom;
        }
    }

    class BottomMutation : RectMutation
    {
        private int amount;
        private Rectangle rect;

        public BottomMutation(int amount, Rectangle rect)
        {
            this.amount = amount;
            this.rect = rect;
        }

        public override Rectangle Mutate()
        {
            return new Rectangle(new Point(rect.Left, rect.Bottom + amount), rect.TopRight);
        }

        public override bool CanMutate()
        {
            return rect.Bottom + amount >= 0 && rect.Bottom + amount < 400 && rect.Bottom + amount < rect.Top;
        }
    }

    class LeftMutation : RectMutation
    {
        private int amount;
        private Rectangle rect;

        public LeftMutation(int amount, Rectangle rect)
        {
            this.amount = amount;
            this.rect = rect;
        }

        public override Rectangle Mutate()
        {
            return new Rectangle(new Point(rect.Left + amount, rect.Bottom), rect.TopRight);
        }

        public override bool CanMutate()
        {
            return rect.Left + amount >= 0 && rect.Left + amount < 400 && rect.Left + amount < rect.Right;
        }
    }

    class RightMutation : RectMutation
    {
        private int amount;
        private Rectangle rect;

        public RightMutation(int amount, Rectangle rect)
        {
            this.amount = amount;
            this.rect = rect;
        }

        public override Rectangle Mutate()
        {
            return new Rectangle(rect.BottomLeft, new Point(rect.Right + amount, rect.Top));
        }

        public override bool CanMutate()
        {
            return rect.Right + amount > 0 && rect.Right + amount <= 400 && rect.Right + amount > rect.Left;
        }
    }
}
