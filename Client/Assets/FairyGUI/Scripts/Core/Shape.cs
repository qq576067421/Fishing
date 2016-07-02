using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class Shape : DisplayObject
	{
		int _type;
		int _lineSize;
		Color _lineColor;
		Color _fillColor;

		public Shape()
		{
			CreateGameObject("Shape");
			graphics = new NGraphics(gameObject);
			graphics.texture = NTexture.Empty;
		}

		public bool empty
		{
			get { return _type == 0; }
		}

		public void DrawRect(int lineSize, Color lineColor, Color fillColor)
		{
			_type = 1;
			_optimizeNotTouchable = false;
			_lineSize = lineSize;
			_lineColor = lineColor;
			_fillColor = fillColor;
			_requireUpdateMesh = true;
		}

		public void DrawEllipse(Color fillColor)
		{
			_type = 2;
			_optimizeNotTouchable = false;
			_fillColor = fillColor;
			_requireUpdateMesh = true;
		}

		public void Clear()
		{
			_type = 0;
			_optimizeNotTouchable = true;
			graphics.ClearMesh();
		}

		public override void Update(UpdateContext context)
		{
			if (_requireUpdateMesh)
			{
				_requireUpdateMesh = false;
				if (_type != 0)
				{
					if (_contentRect.width > 0 && _contentRect.height > 0)
					{
						if (_type == 1)
							graphics.DrawRect(_contentRect, _lineSize, _lineColor, _fillColor);
						else
							graphics.DrawEllipse(_contentRect, _fillColor);
					}
					else
						graphics.ClearMesh();
				}
			}

			base.Update(context);
		}
	}
}
