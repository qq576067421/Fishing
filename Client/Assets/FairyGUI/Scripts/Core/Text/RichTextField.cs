using UnityEngine;
using System.Collections.Generic;
using FairyGUI.Utils;

namespace FairyGUI
{
	public class RichTextField : Container
	{
		public IHtmlPageContext htmlPageContext { get; set; }
		public HtmlParseOptions htmlParseOptions { get; private set; }

		TextField _textField;

		public RichTextField()
		{
			CreateGameObject("RichTextField");

			htmlPageContext = HtmlPageContext.inst;
			htmlParseOptions = new HtmlParseOptions();

			_textField = new TextField();
			_textField._optimizeNotTouchable = false;
			_textField._richTextField = this;
			AddChild(_textField);

			graphics = _textField.graphics;

			onClick.AddCapture(__click);
		}

		public string text
		{
			get { return _textField.text; }
			set { _textField.text = value; }
		}

		public string htmlText
		{
			get { return _textField.htmlText; }
			set { _textField.htmlText = value; }
		}

		public TextFormat textFormat
		{
			get { return _textField.textFormat; }
			set { _textField.textFormat = value; }
		}

		public AlignType align
		{
			get { return _textField.align; }
			set { _textField.align = value; }
		}

		public bool autoSize
		{
			get { return _textField.autoSize; }
			set { _textField.autoSize = value; }
		}

		public bool wordWrap
		{
			get { return _textField.wordWrap; }
			set { _textField.wordWrap = value; }
		}

		public int stroke
		{
			get { return _textField.stroke; }
			set { _textField.stroke = value; }
		}

		public Color strokeColor
		{
			get { return _textField.strokeColor; }
			set { _textField.strokeColor = value; }
		}

		public float textWidth
		{
			get { return _textField.textWidth; }
		}

		public float textHeight
		{
			get { return _textField.textHeight; }
		}

		public IHtmlObject GetHtmlObject(string name)
		{
			return _textField.GetHtmlObject(name);
		}

		override protected void OnSizeChanged()
		{
			_textField.size = this.size;

			base.OnSizeChanged();
		}

		public override Rect GetBounds(DisplayObject targetSpace)
		{
			return _textField.GetBounds(targetSpace);
		}

		void __click(EventContext context)
		{
			Vector3 v = context.inputEvent.position;
			v = this.GlobalToLocal(v);

			HtmlElement link = _textField.GetLink(v);
			if (link != null)
			{
				this.DispatchEvent(onClickLink.type, link.GetString("href"));
				context.StopPropagation();
			}
		}
	}
}
