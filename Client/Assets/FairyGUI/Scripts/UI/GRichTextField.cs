using FairyGUI.Utils;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// GRichTextField class.
	/// </summary>
	public class GRichTextField : GTextField
	{
		RichTextField _richTextField;

		public GRichTextField()
			: base()
		{
		}

		override protected void CreateDisplayObject()
		{
			_richTextField = new RichTextField();
			_richTextField.gOwner = this;
			displayObject = _richTextField;
		}

		/// <summary>
		/// 
		/// </summary>
		override public string text
		{
			set
			{
				if (value == null)
					value = string.Empty;
				_text = value;
				_richTextField.width = this.width;
				if (_ubbEnabled)
					_richTextField.htmlText = UBBParser.inst.Parse(_text);
				else
					_richTextField.htmlText = _text;
				UpdateSize();
			}

			get
			{
				return _text;
			}
		}

		public IHtmlObject GetHtmlObject(string name)
		{
			return _richTextField.GetHtmlObject(name);
		}

		override protected void UpdateAutoSize()
		{
			if (_widthAutoSize)
			{
				_richTextField.autoSize = true;
				_richTextField.wordWrap = false;
			}
			else
			{
				_richTextField.autoSize = false;
				_richTextField.wordWrap = true;
			}
			if (!underConstruct)
				UpdateSize();
		}

		override protected void UpdateSize()
		{
			if (_updatingSize)
				return;

			_updatingSize = true;

			_textWidth = Mathf.CeilToInt(_richTextField.textWidth);
			_textHeight = Mathf.CeilToInt(_richTextField.textHeight);

			float w, h;
			if (_widthAutoSize)
				w = _textWidth;
			else
				w = this.width;

			if (_heightAutoSize)
			{
				h = _textHeight;
				if (!_widthAutoSize)
					_richTextField.height = _textHeight;
			}
			else
			{
				h = this.height;
				if (_textHeight > this.height)
					_textHeight = Mathf.CeilToInt(this.height);
				_richTextField.height = h;
			}

			this.SetSize(Mathf.RoundToInt(w), Mathf.RoundToInt(h));
			DoAlign();

			_updatingSize = false;
		}

		override protected void UpdateTextFormat()
		{
			if (_textFormat.font == null || _textFormat.font.Length == 0)
			{
				TextFormat tf = _richTextField.textFormat;
				tf.CopyFrom(_textFormat);
				tf.font = UIConfig.defaultFont;
				_richTextField.textFormat = tf;
			}
			else
			{
				TextFormat tf = _richTextField.textFormat;
				tf.CopyFrom(_textFormat);
				_richTextField.textFormat = tf;
			}
			_richTextField.stroke = _stroke;
			_richTextField.strokeColor = _strokeColor;

			if (!underConstruct)
				UpdateSize();
		}

		override protected void UpdateTextFieldPassword()
		{
			//not supported
		}

		override protected void DoAlign()
		{
			_richTextField.align = _align;
			if (_verticalAlign == VertAlignType.Top || _textHeight == 0)
				_yOffset = 0;
			else
			{
				float dh = this.height - _textHeight;
				if (dh < 0)
					dh = 0;
				if (_verticalAlign == VertAlignType.Middle)
					_yOffset = Mathf.FloorToInt(dh / 2);
				else
					_yOffset = Mathf.FloorToInt(dh);
			}
			HandlePositionChanged();
		}

		override protected void HandleSizeChanged()
		{
			if (!_updatingSize)
			{
				if (!_widthAutoSize)
				{
					_richTextField.width = this.width;

					float h = _richTextField.textHeight;
					float h2 = this.height;
					if (_heightAutoSize)
					{
						_richTextField.height = h;
						this.height = Mathf.RoundToInt(h);
					}
					else
						_richTextField.height = h2;
				}
				DoAlign();
			}
		}
	}
}
