using System;
using System.Windows;
using System.Windows.Controls;

namespace StepBro.Workbench.Controls
{
    public class LeftRightPanel : Panel
    {
        private readonly double m_verticalSpaceBetweenLeftAndRight = 0.0;
        private readonly double m_verticalSpaceBetweenPairs = 12.0;

        protected override Size MeasureOverride(Size availableSize)
        {
            Size panelDesiredSize = new Size(
                double.IsPositiveInfinity(availableSize.Width) ? 1000.0 : availableSize.Width, availableSize.Height);
            double top = 0;
            bool isRight = false;
            UIElement previous = null;
            foreach (UIElement child in this.InternalChildren)
            {
                child.Measure(availableSize);
                panelDesiredSize.Width = Math.Max(panelDesiredSize.Width, child.DesiredSize.Width);
                if (isRight)
                {
                    if ((previous.DesiredSize.Width + child.DesiredSize.Width + 10) < availableSize.Width)
                    {
                        panelDesiredSize.Height = top + Math.Max(child.DesiredSize.Height + m_verticalSpaceBetweenLeftAndRight, previous.DesiredSize.Height);
                        top = panelDesiredSize.Height + m_verticalSpaceBetweenPairs;
                    }
                    else
                    {
                        panelDesiredSize.Height = top + child.DesiredSize.Height + m_verticalSpaceBetweenLeftAndRight + previous.DesiredSize.Height;
                        top = panelDesiredSize.Height + m_verticalSpaceBetweenPairs;
                    }
                }
                isRight = !isRight;
                previous = child;
            }

            return panelDesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double top = 0.0;
            bool isLeft = true;
            UIElement previous = null;
            foreach (UIElement child in this.InternalChildren)
            {
                if (isLeft)
                {
                    child.Arrange(new Rect(new Point(0.0, top), child.DesiredSize));
                }
                else
                {
                    // Space enough to put'em beside each other?
                    if ((child.DesiredSize.Width + previous.DesiredSize.Width + 10) < finalSize.Width)
                    {
                        var myHeight = child.DesiredSize.Height;
                        if (myHeight > previous.DesiredSize.Height)
                        {
                            child.Arrange(new Rect(new Point(finalSize.Width - child.DesiredSize.Width, top + m_verticalSpaceBetweenLeftAndRight), child.DesiredSize));
                            top += myHeight + (m_verticalSpaceBetweenLeftAndRight + m_verticalSpaceBetweenPairs);
                        }
                        else
                        {
                            var myTop = (top + previous.DesiredSize.Height + m_verticalSpaceBetweenLeftAndRight) - myHeight;
                            child.Arrange(new Rect(new Point(finalSize.Width - child.DesiredSize.Width, myTop), child.DesiredSize));
                            top = myTop + myHeight + m_verticalSpaceBetweenPairs;
                        }
                    }
                    else
                    {
                        top += previous.DesiredSize.Height + m_verticalSpaceBetweenLeftAndRight;
                        child.Arrange(new Rect(new Point(finalSize.Width - child.DesiredSize.Width, top), child.DesiredSize));
                        top += child.DesiredSize.Height + m_verticalSpaceBetweenPairs;
                    }
                }
                previous = child;
                isLeft = !isLeft;
            }
            return finalSize;
        }
    }
}
