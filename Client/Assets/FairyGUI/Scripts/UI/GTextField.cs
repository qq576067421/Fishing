using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class GTextField : GObject, IColorGear
	{
		/// <summary>
		/// 
		/// </summary>
		public EventListener onFocusIn { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onFocusOut { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public EventListener onChanged { get; private set; }

		/// <summary>
		/// 
		/// </summary>
		public GearColor gearColor { get; private set; }

		protected TextField _textField;
		protected string _text;
		protected bool _ubbEnabled;
		protected AutoSizeType _autoSize;
		protected bool _widthAutoSize;
		protected bool _heightAutoSize;
		protected TextFormat _textFormat;
		protected AlignType _align;
		protected VertAlignType _verticalAlign;
		protected int _stroke;
		protected Color _strokeColor;
		protected bool _singleLine;
		protected bool _displayAsPassword;

		protected bool _updatingSize;
		protected int _textWidth;
		protected int _textHeight;

		public GTextField()
			: base()
		{
			underConstruct = true;

			_textFormat = new TextFormat();
			_textFormat.font = UIConfig.defaultFont;
			_textFormat.size = 12;
			_textFormat.color = Color.black;
			_textFormat.lineSpacing = 3;
			_textFormat.letterSpacing = 0;
			_strokeColor = new Color(0, 0, 0, 1);
			UpdateTextFormat();

			_text = string.Empty;
			_align = AlignType.Left;
			_verticalAlign = VertAlignType.Top;

			this.autoSize = AutoSizeType.Both;

			underConstruct = false;

			gearColor = new GearColor(this);

			onFocusIn = new EventListener(this, "onFocusIn");
			onFocusOut = new EventListener(this, "onFocusOut");
			onChanged = new EventListener(this, "onChanged");
		}

		override protected void CreateDisplayObject()
		{
			_textField = new TextField();
			_textField.gOwner = this;
			displayObject = _textField;
		}

		/// <summary>
		/// 
		/// </summary>
		override public string text
		{
			get
			{
				return _text;
			}
			set
			{
				if (value == null)
					value = string.Empty;
				_text = value;
				_textField.width = this.width;
				UpdateTextFieldText();
				UpdateSize();
			}
		}

		virtual protected void UpdateTextFieldText()
		{
			if (_ubbEnabled)
				_textField.htmlText = UBBParser.inst.Parse(XMLUtils.EncodeString(_text));
			else
				_textField.text = _text;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool displayAsPassword
		{
			get { return _displayAsPassword; }
			set
			{
				if (_displayAsPassword != value)
				{
					_displayAsPassword = value;
					UpdateTextFieldPassword();
				}
			}
		}

		virtual protected void UpdateTextFieldPassword()
		{
			_textField.displayAsPassword = _displayAsPassword;
		}

		/// <summary>
		/// 
		/// </summary>
		public TextFormat textFormat
		{
			get
			{
				return _textFormat;
			}
			set
			{
				_textFormat = value;
				UpdateTextFormat();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color color
		{
			get
			{
				return _textFormat.color;
			}
			set
			{
				if (!_textFormat.color.Equals(value))
				{
					_textFormat.color = value;

					if (gearColor.controller != null)
						gearColor.UpdateState();

					UpdateTextFormat();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AlignType align
		{
			get
			{
				return _align;
			}
			set
			{
				if (_align != value)
				{
					_align = value;
					DoAlign();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public VertAlignType verticalAlign
		{
			get
			{
				return _verticalAlign;
			}
			set
			{
				if (_verticalAlign != value)
				{
					_verticalAlign = value;
					DoAlign();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool singleLine
		{
			get { return _singleLine; }
			set
			{
				if (_singleLine != value)
				{
					_singleLine = value;
					UpdateTextFormat();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int stroke
		{
			get
			{
				return _stroke;
			}
			set
			{
				if (_stroke != value)
				{
					_stroke = value;
					UpdateTextFormat();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public Color strokeColor
		{
			get
			{
				return _strokeColor;
			}
			set
			{
				if (_strokeColor != value)
				{
					_strokeColor = value;
					UpdateTextFormat();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool UBBEnabled
		{
			get
			{
				return _ubbEnabled;
			}
			set
			{
				_ubbEnabled = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public AutoSizeType autoSize
		{
			get
			{
				return _autoSize;
			}
			set
			{
				if (_autoSize != value)
				{
					_autoSize = value;
					if (this is GTextInput)
						return;

					_widthAutoSize = value == AutoSizeType.Both;
					_heightAutoSize = value == AutoSizeType.Both || value == AutoSizeType.Height;

					UpdateAutoSize();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public float textWidth
		{
			get { return _textWidth; }
		}

		/// <summary>
		/// 
		/// </summary>
		public float textHeight
		{
			get { return _textHeight; }
		}

		override public void HandleControllerChanged(Controller c)
		{
			base.HandleControllerChanged(c);

			if (gearColor.controller == c)
				gearColor.Apply();
		}

		virtual protected void UpdateAutoSize()
		{
			if (_widthAutoSize)
			{
				_textField.autoSize = true;
				_textField.wordWrap = false;
			}
			else
			{
				_textField.autoSize = false;
				_textField.wordWrap = true;
			}
			if (!underConstruct)
				UpdateSize();
		}

		virtual protected void UpdateSize()
		{
			if (_updatingSize)
				return;

			_updatingSize = true;

			_textWidth = Mathf.CeilToInt(_textField.textWidth);
			_textHeight = Mathf.CeilToInt(_textField.textHeight);

			float w, h;
			if (_widthAutoSize)
				w = _textWidth;
			else
				w = this.width;

			if (_heightAutoSize)
			{
				h = _textHeight;
				if (!_widthAutoSize)
					_textField.height = _textHeight;
			}
			else
			{
				h = this.height;
				if (_textHeight > this.height)
					_textHeight = Mathf.CeilToInt(this.height);
				_textField.height = h;
			}

			this.SetSize(w, h);
			DoAlign();

			_updatingSize = false;
		}

		virtual protected void UpdateTextFormat()
		{
			if (_textFormat.font == null || _textFormat.font.Length == 0)
			{
				TextFormat tf = _textField.textFormat;
				tf.CopyFrom(_textFormat);
				tf.font = UIConfig.defaultFont;
				_textField.textFormat = tf;
			}
			else
			{
				TextFormat tf = _textField.textFormat;
				tf.CopyFrom(_textFormat);
				_textField.textFormat = _textFormat;
			}
			_textField.align = _align;
			_textField.stroke = _stroke;
			_textField.strokeColor = _strokeColor;
			_textField.singleLine = _singleLine;

			if (!underConstruct)
				UpdateSize();
		}

		virtual protected void DoAlign()
		{
			_textField.align = _align;
			if (_verticalAlign == VertAlignType.Top)
				_yOffset = 0;
			else
			{
				float dh;
				if (_textHeight == 0)
					dh = this.height - this.textFormat.size;
				else
					dh = this.height - _textHeight;
				if (dh < 0)
					dh = 0;
				if (_verticalAlign == VertAlignType.Middle)
					_yOffset = dh / 2;
				else
					_yOffset = dh;
			}
			HandlePositionChanged();
		}

		override protected void HandleSizeChanged()
		{
			if (!_updatingSize)
			{
				if (!_widthAutoSize)
				{
					float tw, th;
					tw = this.width;
					float h = _textField.textHeight;
					float h2 = this.height;
					if (_heightAutoSize)
					{
						th = h;
						this.height = Mathf.RoundToInt(h);
					}
					else
						th = h2;
					_textField.SetSize(tw, th);
				}
				DoAlign();
			}
		}

		override public void Setup_BeforeAdd(XML xml)
		{
			base.Setup_BeforeAdd(xml);

			string str;
			this.displayAsPassword = xml.GetAttributeBool("password", false);
			str = xml.GetAttribute("font");
			if (str != null)
				_textFormat.font = str;

			str = xml.GetAttribute("fontSize");
			if (str != null)
				_textFormat.size = int.Parse(str);

			str = xml.GetAttribute("color");
			if (str != null)
				_textFormat.color = ToolSet.ConvertFromHtmlColor(str);

			str = xml.GetAttribute("align");
			if (str != null)
				_align = FieldTypes.ParseAlign(str);

			str = xml.GetAttribute("vAlign");
			if (str != null)
				_verticalAlign = FieldTypes.ParseVerticalAlign(str);

			str = xml.GetAttribute("leading");
			if (str != null)
				_textFormat.lineSpacing = int.Parse(str);

			str = xml.GetAttribute("letterSpacing");
			if (str != null)
				_textFormat.letterSpacing = int.Parse(str);

			_ubbEnabled = xml.GetAttributeBool("ubb", false);

			str = xml.GetAttribute("autoSize");
			if (str != null)
				this.autoSize = FieldTypes.ParseAutoSizeType(str);

			_textFormat.underline = xml.GetAttributeBool("underline", false);
			_textFormat.italic = xml.GetAttributeBool("italic", false);
			_textFormat.bold = xml.GetAttributeBool("bold", false);
			_singleLine = xml.GetAttributeBool("singleLine", false);
			str = xml.GetAttribute("strokeColor");
			if (str != null)
			{
				_strokeColor = ToolSet.ConvertFromHtmlColor(str);
				_stroke = xml.GetAttributeInt("strokeSize", 1);
			}
		}

		override public void Setup_AfterAdd(XML xml)
		{
			base.Setup_AfterAdd(xml);

			XML cxml = xml.GetNode("gearColor");
			if (cxml != null)
				gearColor.Setup(cxml);

			UpdateTextFormat();

			string str = xml.GetAttribute("text");
			if (str != null && str.Length > 0)
				this.text = str;
		}
	}

}
