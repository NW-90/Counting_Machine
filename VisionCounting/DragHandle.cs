using System.Drawing;

namespace DLCounting
{ 

  internal class DragHandle
  {
    #region Public Constructors

    public DragHandle(DragHandleAnchor anchor)
      : this()
    {
      this.Anchor = anchor;
    }

    #endregion

    #region Protected Constructors

    protected DragHandle()
    {
      this.Enabled = true;
      this.Visible = true;
    }

    #endregion

    #region Public Properties

    public DragHandleAnchor Anchor { get; protected set; }

    public Rectangle Bounds { get; set; }

    public bool Enabled { get; set; }

    public bool Visible { get; set; }

    #endregion
  }
}
