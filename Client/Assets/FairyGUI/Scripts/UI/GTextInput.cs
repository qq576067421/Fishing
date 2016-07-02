using FairyGUI.Utils;

namespace FairyGUI
{
	/// <summary>
	/// 
	/// </summary>
	public class GTextInput : GTextField
	{
		string _promptText;

		public GTextInput()
		{
			this.focusable = true;
			_textField.autoSize = false;
			_textField.wordWrap = false;
			_textField.onChanged.AddCapture(__onChanged);
			_textField.onFocusIn.AddCapture(__onFocusIn);
			_textField.onFocusOut.AddCapture(__onFocusOut);
		}

		/// <summary>
		/// 
		/// </summary>
		public bool editable
		{
			get
			{
				return _textField.input;
			}
			set
			{
				_textField.input = false;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int maxLength
		{
			get
			{
				return _textField.maxLength;
			}
			set
			{
				_textField.maxLength = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int caretPosition
		{
			get { return _textField.caretPosition; }
			set { _textField.caretPosition = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string promptText
		{
			get
			{
				return _promptText;
			}
			set
			{
				_promptText = value;
				UpdateTextFieldText();
			}
		}

		/// <summary>
		/// <see cref="UnityEngine.TouchScreenKeyboardType"/>
		/// </summary>
		public int keyboardType
		{
			get { return _textField.inputAdapter.keyboardType; }
			set { _textField.inputAdapter.keyboardType = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="value"></param>
		public void ReplaceSelection(string value)
		{
			_textField.ReplaceSelection(value);
			UpdateTextFieldText();
			UpdateSize();
		}

		override protected void CreateDisplayObject()
		{
			base.CreateDisplayObject();

			_textField.input = true;
		}

		public override void Setup_BeforeAdd(Utils.XML xml)
		{
			base.Setup_BeforeAdd(xml);

			_promptText = xml.GetAttribute("prompt");
		}

		public override void Setup_AfterAdd(XML xml)
		{
			base.Setup_AfterAdd(xml);

			if (string.IsNullOrEmpty(_text))
			{
				if (!string.IsNullOrEmpty(_promptText))
				{
					_textField.displayAsPassword = false;
					_textField.htmlText = UBBParser.inst.Parse(XMLUtils.EncodeString(_promptText));
				}
				else if (_verticalAlign != VertAlignType.Top)
					DoAlign();
			}
		}

		override protected void UpdateTextFieldText()
		{
			if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_promptText))
			{
				_textField.displayAsPassword = false;
				_textField.htmlText = UBBParser.inst.Parse(XMLUtils.EncodeString(_promptText));
			}
			else
			{
				_textField.displayAsPassword = _displayAsPassword;
				_textField.text = _text;
			}
		}

		void __onChanged(EventContext context)
		{
			_text = _textField.text;
			UpdateSize();
		}

		void __onFocusIn(EventContext context)
		{
			if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_promptText))
			{
				_textField.displayAsPassword = _displayAsPassword;
				_textField.text = string.Empty;
			}
		}

		void __onFocusOut(EventContext context)
		{
			_text = _textField.text;
			if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_promptText))
			{
				_textField.displayAsPassword = false;
				_textField.htmlText = UBBParser.inst.Parse(XMLUtils.EncodeString(_promptText));
			}
		}
	}
}