using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using FairyGUI.Utils;

namespace FairyGUI
{
	public class TextField : DisplayObject
	{
		public EventListener onFocusIn { get; private set; }
		public EventListener onFocusOut { get; private set; }
		public EventListener onChanged { get; private set; }

		AlignType _align;
		TextFormat _textFormat;
		bool _input;
		string _text;
		bool _autoSize;
		bool _wordWrap;
		bool _displayAsPassword;
		bool _singleLine;
		int _maxLength;
		bool _html;
		int _caretPosition;
		CharPosition? _selectionStart;
		InputCaret _caret;
		Highlighter _highlighter;
		int _stroke;
		Color _strokeColor;

		List<HtmlElement> _elements;
		List<LineInfo> _lines;
		internal RichTextField _richTextField;

#pragma warning disable 0649
		IMobileInputAdapter _mobileInputAdapter;
#pragma warning restore 0649

		BaseFont _font;
		float _textWidth;
		float _textHeight;
		bool _textChanged;

		const int GUTTER_X = 2;
		const int GUTTER_Y = 2;
		const char E_TAG = (char)1;
		static float[] STROKE_OFFSET = new float[]
		{
			 -1f, 0f, 1f, 0f,
			0f, -1f, 0f, 1f
		};
		static float[] BOLD_OFFSET = new float[]
		{
			-0.5f, 0f, 0.5f, 0f,
			0f, -0.5f, 0f, 0.5f
		};

		public TextField()
		{
			_optimizeNotTouchable = true;

			_textFormat = new TextFormat();
			_textFormat.size = 12;
			_textFormat.lineSpacing = 3;
			_strokeColor = new Color(0, 0, 0, 1);

			_wordWrap = true;
			_displayAsPassword = false;
			_maxLength = int.MaxValue;
			_text = string.Empty;

			_elements = new List<HtmlElement>(1);
			_lines = new List<LineInfo>(1);

			CreateGameObject("TextField");
			graphics = new NGraphics(gameObject);

			onFocusIn = new EventListener(this, "onFocusIn");
			onFocusOut = new EventListener(this, "onFocusOut");
			onChanged = new EventListener(this, "onChanged");
		}

		public TextFormat textFormat
		{
			get { return _textFormat; }
			set
			{
				_textFormat = value;

				string fontName = _textFormat.font;
				if (_font == null || _font.name != fontName)
				{
					_font = FontManager.GetFont(fontName);
					if (_font != null)
						graphics.SetShaderAndTexture(_font.shader, _font.mainTexture);
				}
				if (!string.IsNullOrEmpty(_text))
					_textChanged = true;
			}
		}

		internal BaseFont font
		{
			get { return _font; }
		}

		public AlignType align
		{
			get { return _align; }
			set
			{
				if (_align != value)
				{
					_align = value;
					if (!string.IsNullOrEmpty(_text))
						_textChanged = true;
				}
			}
		}

		public bool input
		{
			get { return _input; }
			set
			{
				if (_input != value)
				{
					_input = value;
					_optimizeNotTouchable = !_input;

					if (_input)
					{
						onFocusIn.Add(__focusIn);
						onFocusOut.AddCapture(__focusOut);

						if (Stage.touchScreen && _mobileInputAdapter == null)
						{
#if !(UNITY_WEBPLAYER || UNITY_WEBGL || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR)
							_mobileInputAdapter = new MobileInputAdapter();
#endif
						}
					}
					else
					{
						onFocusIn.Remove(__focusIn);
						onFocusOut.RemoveCapture(__focusOut);
					}
				}
			}
		}

		public IMobileInputAdapter inputAdapter
		{
			get { return _mobileInputAdapter; }
		}

		public string text
		{
			get { return _text; }
			set
			{
				_text = value;
				_textChanged = true;
				_html = false;
				ClearSelection();
				if (_caretPosition > _text.Length)
					_caretPosition = _text.Length;
			}
		}

		public string htmlText
		{
			get { return _text; }
			set
			{
				_text = value;
				_textChanged = true;
				_html = true;
				ClearSelection();
				if (_caretPosition > _text.Length)
					_caretPosition = _text.Length;
			}
		}

		public bool autoSize
		{
			get { return _autoSize; }
			set
			{
				if (_autoSize != value)
				{
					_autoSize = value;
					_textChanged = true;
				}
			}
		}

		public bool wordWrap
		{
			get { return _wordWrap; }
			set { _wordWrap = value; _textChanged = true; }
		}

		public bool singleLine
		{
			get { return _singleLine; }
			set { _singleLine = value; _textChanged = true; }
		}

		public bool displayAsPassword
		{
			get { return _displayAsPassword; }
			set { _displayAsPassword = value; }
		}

		public int maxLength
		{
			get { return _maxLength; }
			set { _maxLength = value; }
		}

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
					_requireUpdateMesh = true;
				}
			}
		}

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
					_requireUpdateMesh = true;
				}
			}
		}

		public float textWidth
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _textWidth;
			}
		}
		public float textHeight
		{
			get
			{
				if (_textChanged)
					BuildLines();

				return _textHeight;
			}
		}

		override protected void OnSizeChanged()
		{
			if (_wordWrap)
				_textChanged = true;

			base.OnSizeChanged();
		}

		public override Rect GetBounds(DisplayObject targetSpace)
		{
			if (_textChanged && _autoSize)
				BuildLines();

			return base.GetBounds(targetSpace);
		}

		public int caretPosition
		{
			get { return _caretPosition; }
			set
			{
				_caretPosition = value;
				if (_caretPosition > _text.Length)
					_caretPosition = _text.Length;

				if (_caret != null)
				{
					_selectionStart = null;
					AdjustCaret(GetCharPosition(_caretPosition));
				}
			}
		}

		public void ReplaceSelection(string value)
		{
			if (!_input)
				return;

			InsertText(value);
		}

		public override void Update(UpdateContext context)
		{
			if (_mobileInputAdapter != null)
			{
				if (_mobileInputAdapter.done)
				{
					UpdateContext.OnEnd += () =>
					{
						//将促使一个onFocusOut事件的调用。如果不focus-out，则在键盘关闭后，玩家再次点击文本，focus-in不会触发，键盘不会打开
						//另外也使开发者有机会得到一个键盘关闭的通知
						Stage.inst.focus = null;
					};
				}

				string s = _mobileInputAdapter.GetInput();

				if (s != null && s != _text)
				{
					if (s.Length > _maxLength)
						s = s.Substring(0, _maxLength);
					this.text = s;
					UpdateContext.OnEnd += () => { onChanged.Call(); };
				}
			}

			if (_caret != null)
			{
				string s = Input.inputString;
				if (!string.IsNullOrEmpty(s))
				{
					StringBuilder sb = new StringBuilder();
					for (int i = 0; i < s.Length; ++i)
					{
						char ch = s[i];
						if (ch >= ' ') sb.Append(ch.ToString());
					}
					if (sb.Length > 0)
					{
						s = sb.ToString();
						if (_text.Length + s.Length > _maxLength)
							s = s.Substring(0, _text.Length - maxLength);
						InsertText(s);
					}
				}
			}

			if (_font != null)
			{
				if (_font.mainTexture != graphics.texture)
				{
					if (!_textChanged)
						RequestText();
					graphics.texture = _font.mainTexture;
					_requireUpdateMesh = true;
				}

				if (_textChanged)
					BuildLines();

				if (_requireUpdateMesh)
					BuildMesh();
			}

			if (_input)
			{
				Rect rect = _contentRect;
				rect.x += GUTTER_X;
				rect.y += GUTTER_Y;
				rect.width -= GUTTER_X * 2;
				rect.height -= GUTTER_Y * 2;
				context.EnterClipping(this.id, this.TransformRect(rect, null), null);

				base.Update(context);

				if (_highlighter != null)
					_highlighter.grahpics.UpdateMaterial(context);

				context.LeaveClipping();

				if (_caret != null) //不希望光标发生剪切，所以放到LeaveClipping后
				{
					_caret.grahpics.UpdateMaterial(context);
					_caret.Blink();
				}
			}
			else
				base.Update(context);
		}

		//准备字体纹理
		void RequestText()
		{
			int count = _elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = _elements[i];
				if (element.type == HtmlElementType.Text)
				{
					_font.SetFormat(element.format);
					_font.PrepareCharacters(element.text);
					_font.PrepareCharacters("_-*");
				}
			}
		}

		FontStyle GetFontStyle(TextFormat format)
		{
			if (format.bold)
			{
				if (format.italic)
					return FontStyle.BoldAndItalic;
				else
					return FontStyle.Bold;
			}
			else
			{
				if (format.italic)
					return FontStyle.Italic;
				else
					return FontStyle.Normal;
			}
		}

		void BuildLines()
		{
			Cleanup();

			_textChanged = false;
			_requireUpdateMesh = true;

			if (_caret != null)
				_caret.SetSizeAndColor(_textFormat.size, _textFormat.color);

			if (_text.Length == 0)
			{
				SetEmpty();
				return;
			}

			if (_displayAsPassword)
			{
				int textLen = _text.Length;
				StringBuilder tmp = new StringBuilder(textLen);
				for (int i = 0; i < textLen; i++)
					tmp.Append("*");

				HtmlElement element = HtmlElement.GetElement(HtmlElementType.Text);
				element.text = tmp.ToString();
				element.format.CopyFrom(_textFormat);
				_elements.Add(element);
			}
			else if (_html)
				HtmlParser.inst.Parse(_text, _textFormat, _elements, _richTextField != null ? _richTextField.htmlParseOptions : null);
			else
			{
				HtmlElement element = HtmlElement.GetElement(HtmlElementType.Text);
				element.text = _text;
				element.format.CopyFrom(_textFormat);
				_elements.Add(element);
			}

			if (_elements.Count == 0)
			{
				SetEmpty();
				return;
			}

			int letterSpacing = _textFormat.letterSpacing;
			int lineSpacing = _textFormat.lineSpacing - 1;
			float rectWidth = _contentRect.width - GUTTER_X * 2;
			float lineWidth = 0, lineHeight = 0, lineTextHeight = 0;
			int glyphWidth = 0, glyphHeight = 0;
			int wordChars = 0;
			float wordStart = 0, wordEnd = 0;
			float lastLineHeight = 0;
			TextFormat format = _textFormat;
			_font.SetFormat(format);
			bool wrap;
			if (_input)
			{
				letterSpacing++;
				wrap = !_singleLine;
			}
			else
				wrap = _wordWrap && !_singleLine;
			float lineY = GUTTER_Y;

			RequestText();

			LineInfo line;
			StringBuilder lineBuffer = new StringBuilder();

			int count = _elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = _elements[i];

				if (_html)
				{
					//Special tag, indicates the start of an element
					lineBuffer.Append(E_TAG);
					lineBuffer.Append((char)(i + 33));
					if (wordChars > 0)
						wordEnd = lineWidth;
					wordChars = 0;
				}

				if (element.type == HtmlElementType.Text)
				{
					format = element.format;
					_font.SetFormat(format);

					int textLength = element.text.Length;
					for (int offset = 0; offset < textLength; ++offset)
					{
						char ch = element.text[offset];
						if (ch == E_TAG)
							ch = '?';

						if (ch == '\n')
						{
							lineBuffer.Append(ch);
							line = LineInfo.Borrow();
							line.width = lineWidth;
							if (lineTextHeight == 0)
							{
								if (lastLineHeight == 0)
									lastLineHeight = format.size;
								if (lineHeight == 0)
									lineHeight = lastLineHeight;
								lineTextHeight = lineHeight;
							}
							line.height = lineHeight;
							lastLineHeight = lineHeight;
							line.textHeight = lineTextHeight;
							line.text = lineBuffer.ToString();
							line.y = lineY;
							lineY += (line.height + lineSpacing);
							if (line.width > _textWidth)
								_textWidth = line.width;
							_lines.Add(line);
							lineBuffer.Length = 0;
							lineWidth = 0;
							lineHeight = 0;
							lineTextHeight = 0;
							wordChars = 0;
							wordStart = 0;
							wordEnd = 0;
							continue;
						}

						if (ch > 256 || ch <= ' ')
						{
							if (wordChars > 0)
								wordEnd = lineWidth;
							wordChars = 0;
						}
						else
						{
							if (wordChars == 0)
								wordStart = lineWidth;
							wordChars++;
						}

						if (_font.GetGlyphSize(ch, out glyphWidth, out glyphHeight))
						{
							if (glyphHeight > lineTextHeight)
								lineTextHeight = glyphHeight;

							if (glyphHeight > lineHeight)
								lineHeight = glyphHeight;

							if (lineWidth != 0)
								lineWidth += letterSpacing;
							lineWidth += glyphWidth;
						}

						if (!wrap || lineWidth <= rectWidth)
						{
							lineBuffer.Append(ch);
						}
						else
						{
							line = LineInfo.Borrow();
							line.height = lineHeight;
							line.textHeight = lineTextHeight;

							if (lineBuffer.Length == 0) //the line cannt fit even a char
							{
								line.text = ch.ToString();
							}
							else if (wordChars > 0 && wordEnd > 0) //if word had broken, move it to new line
							{
								lineBuffer.Append(ch);
								int len = lineBuffer.Length - wordChars;
								line.text = lineBuffer.ToString(0, len);
								if (!_input)
									line.text = line.text.TrimEnd();
								line.width = wordEnd;
								lineBuffer.Remove(0, len);

								lineWidth -= wordStart;
							}
							else
							{
								line.text = lineBuffer.ToString();
								line.width = lineWidth - (glyphWidth + letterSpacing);
								lineBuffer.Length = 0;

								lineBuffer.Append(ch);
								lineWidth = glyphWidth;
								lineHeight = glyphHeight;
								lineTextHeight = glyphHeight;
							}
							line.y = lineY;
							lineY += (line.height + lineSpacing);
							if (line.width > _textWidth)
								_textWidth = line.width;

							wordChars = 0;
							wordStart = 0;
							wordEnd = 0;
							_lines.Add(line);
						}
					}
				}
				else
				{
					IHtmlObject htmlObject = null;
					if (_richTextField != null)
					{
						element.space = (int)(rectWidth - lineWidth);
						htmlObject = _richTextField.htmlPageContext.CreateObject(_richTextField, element);
						element.htmlObject = htmlObject;
					}
					if (htmlObject != null)
					{
						glyphWidth = (int)htmlObject.width + 2;
						glyphHeight = (int)htmlObject.height;
					}
					else
					{
						glyphWidth = 0;
						glyphHeight = 0;
					}

					if (glyphHeight > lineHeight)
						lineHeight = glyphHeight;

					if (lineWidth != 0)
						lineWidth += letterSpacing;
					lineWidth += glyphWidth;

					if (wrap && lineWidth > rectWidth && glyphWidth < rectWidth)
					{
						line = LineInfo.Borrow();
						line.height = lineHeight;
						line.textHeight = lineTextHeight;
						int len = lineBuffer.Length;
						line.text = lineBuffer.ToString(0, len - 2);
						line.width = lineWidth - (glyphWidth + letterSpacing);
						lineBuffer.Remove(0, len - 2);
						lineWidth = glyphWidth;
						line.y = lineY;
						lineY += (line.height + lineSpacing);
						if (line.width > _textWidth)
							_textWidth = line.width;

						lineTextHeight = 0;
						lineHeight = glyphHeight;
						wordChars = 0;
						wordStart = 0;
						wordEnd = 0;
						_lines.Add(line);
					}
				}
			}

			if (lineBuffer.Length > 0 || _lines.Count > 0 && _lines[_lines.Count - 1].text.EndsWith("\n"))
			{
				line = LineInfo.Borrow();
				line.width = lineWidth;
				if (lineHeight == 0)
					lineHeight = lastLineHeight;
				if (lineTextHeight == 0)
					lineTextHeight = lineHeight;
				line.height = lineHeight;
				line.textHeight = lineTextHeight;
				line.text = lineBuffer.ToString();
				line.y = lineY;
				if (line.width > _textWidth)
					_textWidth = line.width;
				_lines.Add(line);
			}

			if (_textWidth > 0)
				_textWidth += GUTTER_X * 2;

			count = _lines.Count;
			line = _lines[_lines.Count - 1];
			_textHeight = line.y + line.height + GUTTER_Y;
			if (_autoSize)
			{
				_contentRect.width = _textWidth;
				_contentRect.height = _textHeight;
				base.OnSizeChanged();
			}
		}

		void SetEmpty()
		{
			LineInfo emptyLine = LineInfo.Borrow();
			emptyLine.width = 0;
			emptyLine.height = 0;
			emptyLine.text = string.Empty;
			emptyLine.y = GUTTER_Y;
			_lines.Add(emptyLine);

			_textWidth = 0;
			_textHeight = 0;
			if (_autoSize)
			{
				_contentRect.width = _textWidth;
				_contentRect.height = _textHeight;
				base.OnSizeChanged();
			}
		}

		static List<Vector3> sCachedVerts = new List<Vector3>();
		static List<Vector2> sCachedUVs = new List<Vector2>();
		static List<Color32> sCachedCols = new List<Color32>();
		void BuildMesh()
		{
			_requireUpdateMesh = false;

			if (_textWidth == 0)
			{
				graphics.ClearMesh();
				if (_caret != null)
				{
					_caretPosition = 0;
					CharPosition cp = GetCharPosition(_caretPosition);
					AdjustCaret(cp);
				}
				return;
			}

			int letterSpacing = _textFormat.letterSpacing;
			float rectWidth = _contentRect.width - GUTTER_X * 2;
			TextFormat format = _textFormat;
			_font.SetFormat(format);
			Color32 color = format.color;
			bool customBold = _font.customBold;
			if (_input)
			{
				letterSpacing++;
				customBold = false;
			}

			Vector3 v0 = Vector3.zero, v1 = Vector3.zero;
			Vector2 u0, u1, u2, u3;
			float specFlag;

			List<Vector3> vertList = sCachedVerts;
			List<Vector2> uvList = sCachedUVs;
			List<Color32> colList = sCachedCols;
			vertList.Clear();
			uvList.Clear();
			colList.Clear();

			HtmlElement currentLink = null;

			float charX;
			float tmpX;
			float lineIndent;
			float charIndent = 0;
			bool hasObject = false;

			int lineCount = _lines.Count;
			for (int i = 0; i < lineCount; ++i)
			{
				LineInfo line = _lines[i];

				if (_align == AlignType.Center)
					lineIndent = (int)((rectWidth - line.width) / 2);
				else if (_align == AlignType.Right)
					lineIndent = rectWidth - line.width;
				else
					lineIndent = 0;

				charX = GUTTER_X + lineIndent;

				int textLength = line.text.Length;
				for (int j = 0; j < textLength; j++)
				{
					char ch = line.text[j];
					if (ch == E_TAG)
					{
						int elementIndex = (int)line.text[++j] - 33;
						HtmlElement element = _elements[elementIndex];
						if (element.type == HtmlElementType.Text)
						{
							format = element.format;
							_font.SetFormat(format);
							color = format.color;
						}
						else if (element.type == HtmlElementType.LinkStart)
						{
							currentLink = element;
							currentLink.quadStart = vertList.Count / 4;
						}
						else if (element.type == HtmlElementType.LinkEnd)
						{
							if (currentLink != null)
							{
								currentLink.quadEnd = vertList.Count / 4;
								currentLink = null;
							}
						}
						else
						{
							IHtmlObject htmlObj = element.htmlObject;
							if (htmlObj != null)
							{
								htmlObj.SetPosition(charX + 1, line.y + (int)((line.height - htmlObj.height) / 2));
								hasObject = true;
								charX += htmlObj.width + letterSpacing + 2;
							}
						}
						continue;
					}

					if (ch == ' ')
					{
						if (format.underline)
							ch = '_';
					}

					GlyphInfo glyph = _font.GetGlyph(ch);
					if (glyph != null)
					{
						tmpX = charX;
						charIndent = (int)((line.height + line.textHeight) / 2) - glyph.height;
						v0.x = charX + glyph.vert.xMin;
						v0.y = -line.y - charIndent + glyph.vert.yMin;
						v1.x = charX + glyph.vert.xMax;
						v1.y = -line.y - charIndent + glyph.vert.yMax;
						u0 = glyph.uvBottomLeft;
						u1 = glyph.uvTopLeft;
						u2 = glyph.uvTopRight;
						u3 = glyph.uvBottomRight;
						specFlag = 0;

						if (_font.hasChannel)
						{
							//对于由BMFont生成的字体，使用这个特殊的设置告诉着色器告诉用的是哪个通道
							specFlag = 10 * (glyph.channel == 0 ? 3 : (glyph.channel - 1));
							u0.x += specFlag;
							u1.x += specFlag;
							u2.x += specFlag;
							u3.x += specFlag;
						}
						else if (_font.canLight && format.bold)
						{
							//对于动态字体，使用这个特殊的设置告诉着色器这个文字不需要点亮（粗体亮度足够，不需要）
							specFlag = 10;
							u0.x += specFlag;
							u1.x += specFlag;
							u2.x += specFlag;
							u3.x += specFlag;
						}

						if (!format.bold || !customBold)
						{
							uvList.Add(u0);
							uvList.Add(u1);
							uvList.Add(u2);
							uvList.Add(u3);

							vertList.Add(v0);
							vertList.Add(new Vector3(v0.x, v1.y));
							vertList.Add(new Vector3(v1.x, v1.y));
							vertList.Add(new Vector3(v1.x, v0.y));
							line.quadCount++;

							colList.Add(color);
						}
						else
						{
							for (int b = 0; b < 4; b++)
							{
								uvList.Add(u0);
								uvList.Add(u1);
								uvList.Add(u2);
								uvList.Add(u3);

								float fx = BOLD_OFFSET[b * 2];
								float fy = BOLD_OFFSET[b * 2 + 1];

								vertList.Add(new Vector3(v0.x + fx, v0.y + fy));
								vertList.Add(new Vector3(v0.x + fx, v1.y + fy));
								vertList.Add(new Vector3(v1.x + fx, v1.y + fy));
								vertList.Add(new Vector3(v1.x + fx, v0.y + fy));
								line.quadCount++;

								colList.Add(color);
							}
						}

						charX += letterSpacing + glyph.width;

						if (format.underline)
						{
							glyph = _font.GetGlyph('_');
							if (glyph == null)
								continue;

							//取中点的UV
							if (glyph.uvBottomLeft.x != glyph.uvBottomRight.x)
								u0.x = (glyph.uvBottomLeft.x + glyph.uvBottomRight.x) * 0.5f;
							else
								u0.x = (glyph.uvBottomLeft.x + glyph.uvTopLeft.x) * 0.5f;
							u0.x += specFlag;

							if (glyph.uvBottomLeft.y != glyph.uvTopLeft.y)
								u0.y = (glyph.uvBottomLeft.y + glyph.uvTopLeft.y) * 0.5f;
							else
								u0.y = (glyph.uvBottomLeft.y + glyph.uvBottomRight.y) * 0.5f;

							uvList.Add(u0);
							uvList.Add(u0);
							uvList.Add(u0);
							uvList.Add(u0);

							v0.y = -line.y - charIndent + glyph.vert.yMin - 1;
							v1.y = -line.y - charIndent + glyph.vert.yMax - 1;

							vertList.Add(new Vector3(tmpX, v0.y));
							vertList.Add(new Vector3(tmpX, v1.y));
							vertList.Add(new Vector3(charX, v1.y));
							vertList.Add(new Vector3(charX, v0.y));
							line.quadCount++;

							colList.Add(color);
						}
					}
					else //if (glyph != null)
					{
						v0.x = charX;
						v0.y = -line.y;
						v1.x = v0.x;
						v1.y = v0.y - 1;

						uvList.Add(Vector2.zero);
						uvList.Add(Vector2.zero);
						uvList.Add(Vector2.zero);
						uvList.Add(Vector2.zero);

						vertList.Add(v0);
						vertList.Add(new Vector3(v0.x, v1.y));
						vertList.Add(v1);
						vertList.Add(new Vector3(v1.x, v0.y));
						line.quadCount++;

						colList.Add(color);

						charX += letterSpacing;
					}
				}//text loop
			}//line loop

			if (!_input && _stroke != 0 && _font.canOutline)
			{
				int count = vertList.Count;
				graphics.Alloc(count * 5);
				Vector3[] vertBuf = graphics.vertices;
				Vector2[] uvBuf = graphics.uv;
				Color32[] colBuf = graphics.colors;

				int start = count * 4;
				vertList.CopyTo(0, vertBuf, start, count);
				uvList.CopyTo(0, uvBuf, start, count);
				if (_font.canTint)
				{
					for (int i = 0; i < count; i++)
						colBuf[start + i] = colList[i / 4];
				}
				else
				{
					for (int i = 0; i < count; i++)
						colBuf[start + i] = Color.white;
				}

				Color32 col = _strokeColor;
				int offset;
				for (int j = 0; j < 4; j++)
				{
					offset = j * count;
					for (int i = 0; i < count; i++)
					{
						Vector3 vert = vertList[i];
						Vector2 u = uvList[i];

						//使用这个特殊的设置告诉着色器这个是描边
						if (_font.canOutline)
							u.y = 10 + u.y;
						uvBuf[offset] = u;
						vertBuf[offset] = new Vector3(vert.x + STROKE_OFFSET[j * 2] * _stroke, vert.y + STROKE_OFFSET[j * 2 + 1] * _stroke, 0);
						colBuf[offset] = col;
						offset++;
					}
				}
			}
			else
			{
				int count = vertList.Count;
				graphics.Alloc(count);
				vertList.CopyTo(0, graphics.vertices, 0, count);
				uvList.CopyTo(0, graphics.uv, 0, count);
				if (_font.canTint)
				{
					for (int i = 0; i < count; i++)
						graphics.colors[i] = colList[i / 4];
				}
				else
				{
					for (int i = 0; i < count; i++)
						graphics.colors[i] = Color.white;
				}
			}

			graphics.FillTriangles();
			graphics.UpdateMesh();

			if (hasObject)
				UpdateContext.OnEnd += AddObjects;

			if (_caret != null)
			{
				if (_caretPosition > _text.Length)
					_caretPosition = _text.Length;

				CharPosition cp = GetCharPosition(_caretPosition);
				AdjustCaret(cp);
			}
		}

		void AddObjects()
		{
			int count = _elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = _elements[i];
				if (element.htmlObject != null)
					element.htmlObject.Add();
			}
		}

		void Cleanup()
		{
			if (_richTextField != null)
			{
				int count = _elements.Count;
				for (int i = 0; i < count; i++)
				{
					HtmlElement element = _elements[i];
					if (element.htmlObject != null)
					{
						element.htmlObject.Remove();
						_richTextField.htmlPageContext.FreeObject(element.htmlObject);
					}
				}
			}

			HtmlElement.ReturnElements(_elements);
			LineInfo.Return(_lines);
			_textWidth = 0;
			_textHeight = 0;
		}

		CharPosition GetCharPosition(int charIndex)
		{
			CharPosition cp;
			cp.charIndex = charIndex;

			LineInfo line;
			int lineCount = _lines.Count;
			int i;
			int len;
			for (i = 0; i < lineCount; i++)
			{
				line = _lines[i];
				len = line.text.Length;
				if (charIndex - len < 0)
					break;

				charIndex -= len;
			}
			if (i == lineCount)
				i = lineCount - 1;

			cp.lineIndex = i;
			return cp;
		}

		CharPosition GetCharPosition(Vector3 location)
		{
			CharPosition result;
			int lineCount = _lines.Count;
			int charIndex = 0;
			LineInfo line;
			int last = 0;
			int i;
			for (i = 0; i < lineCount; i++)
			{
				line = _lines[i];
				charIndex += last;

				if (line.y + line.height > location.y)
					break;

				last = line.text.Length;
			}
			if (i == lineCount)
				i = lineCount - 1;

			result.lineIndex = i;
			line = _lines[i];
			int textLen = line.text.Length;
			Vector3 v;
			if (textLen > 0)
			{
				for (i = 0; i < textLen; i++)
				{
					v = graphics.vertices[charIndex * 4 + 2];
					if (v.x > location.x)
						break;

					charIndex++;
				}
				if (i == textLen && result.lineIndex != lineCount - 1)
					charIndex--;
			}

			result.charIndex = charIndex;
			return result;
		}

		void ClearSelection()
		{
			if (_selectionStart != null)
			{
				if (_highlighter != null)
					_highlighter.Clear();
				_selectionStart = null;
			}
		}

		void DeleteSelection()
		{
			if (_selectionStart == null)
				return;

			CharPosition cp = (CharPosition)_selectionStart;
			if (cp.charIndex < _caretPosition)
			{
				this.text = _text.Substring(0, cp.charIndex) + _text.Substring(_caretPosition);
				_caretPosition = cp.charIndex;
			}
			else
				this.text = _text.Substring(0, _caretPosition) + _text.Substring(cp.charIndex);
			ClearSelection();
		}

		string GetSelection()
		{
			if (_selectionStart == null)
				return string.Empty;

			CharPosition cp = (CharPosition)_selectionStart;
			if (cp.charIndex < _caretPosition)
				return _text.Substring(cp.charIndex, _caretPosition - cp.charIndex);
			else
				return _text.Substring(_caretPosition, cp.charIndex - _caretPosition);
		}

		void InsertText(string value)
		{
			if (_selectionStart != null)
				DeleteSelection();
			this.text = _text.Substring(0, _caretPosition) + value + _text.Substring(_caretPosition);
			_caretPosition += value.Length;
			onChanged.Call();
		}

		Vector2 GetCharLocation(CharPosition cp)
		{
			LineInfo line = _lines[cp.lineIndex];
			Vector2 pos;
			if (line.text.Length == 0)
			{
				if (_align == AlignType.Center)
					pos.x = _contentRect.width / 2;
				else
					pos.x = GUTTER_X;
			}
			else if (cp.charIndex == 0 || cp.charIndex < text.Length)
			{
				pos = graphics.vertices[cp.charIndex * 4];
				pos.x -= 1;
			}
			else
				pos = graphics.vertices[(cp.charIndex - 1) * 4 + 2];
			pos.y = line.y;
			return pos;
		}

		void AdjustCaret(CharPosition cp)
		{
			_caretPosition = cp.charIndex;
			Vector2 pos = GetCharLocation(cp);

			Vector2 offset = _positionOffset;
			if (pos.x - offset.x < 5)
			{
				float move = pos.x - (int)Math.Min(50, _contentRect.width / 2);
				if (move < 0)
					move = 0;
				else if (move + _contentRect.width > _textWidth)
					move = Math.Max(0, _textWidth - _contentRect.width);
				offset.x = move;
			}
			else if (pos.x - offset.x > _contentRect.width - 5)
			{
				float move = pos.x - (int)Math.Min(50, _contentRect.width / 2);
				if (move < 0)
					move = 0;
				else if (move + _contentRect.width > _textWidth)
					move = Math.Max(0, _textWidth - _contentRect.width);
				offset.x = move;
			}

			LineInfo line = _lines[cp.lineIndex];
			if (line.y - offset.y < 0)
			{
				float move = line.y - GUTTER_Y;
				offset.y = move;
			}
			else if (line.y + line.height - offset.y >= _contentRect.height)
			{
				float move = line.y + line.height + GUTTER_Y - _contentRect.height;
				if (move < 0)
					move = 0;
				offset.y = move;
			}
			this.SetPositionOffset(offset);

			_caret.SetPosition(pos);

			if (_selectionStart != null)
				UpdateHighlighter(cp);
		}

		void UpdateHighlighter(CharPosition cp)
		{
			CharPosition start = (CharPosition)_selectionStart;
			if (start.charIndex > cp.charIndex)
			{
				CharPosition tmp = start;
				start = cp;
				cp = tmp;
			}

			LineInfo line1;
			LineInfo line2;
			Vector2 v1, v2;
			line1 = _lines[start.lineIndex];
			line2 = _lines[cp.lineIndex];
			v1 = GetCharLocation(start);
			v2 = GetCharLocation(cp);

			_highlighter.BeginUpdate();
			if (start.lineIndex == cp.lineIndex)
			{
				Rect r = Rect.MinMaxRect(v1.x, line1.y, v2.x, line1.y + line1.height);
				_highlighter.AddRect(r);
			}
			else
			{
				Rect r = Rect.MinMaxRect(v1.x, line1.y, _contentRect.width - GUTTER_X * 2, line1.y + line1.height);
				_highlighter.AddRect(r);

				for (int i = start.lineIndex + 1; i < cp.lineIndex; i++)
				{
					LineInfo line = _lines[i];
					r = Rect.MinMaxRect(GUTTER_X, line.y, _contentRect.width - GUTTER_X * 2, line.y + line.height);
					if (i == start.lineIndex)
						r.yMin = line1.y + line1.height;
					if (i == cp.lineIndex - 1)
						r.yMax = line2.y;
					_highlighter.AddRect(r);
				}

				r = Rect.MinMaxRect(GUTTER_X, line2.y, v2.x, line2.y + line2.height);
				_highlighter.AddRect(r);
			}
			_highlighter.EndUpdate();
		}

		internal HtmlElement GetLink(Vector2 pos)
		{
			if (_elements == null)
				return null;

			pos += this._positionOffset;

			if (!_contentRect.Contains(pos))
				return null;

			Vector3[] verts = graphics.vertices;
			int count = graphics.vertCount / 4;
			pos.y = -pos.y;
			int i;
			for (i = 0; i < count; i++)
			{
				Vector3 vertBottomLeft = verts[i * 4];
				Vector3 vertTopRight = verts[i * 4 + 2];
				if (pos.y > vertBottomLeft.y && pos.y <= vertTopRight.y && vertTopRight.x > pos.x)
					break;
			}
			if (i == count)
				return null;

			int quadIndex = i;
			count = _elements.Count;
			for (i = 0; i < count; i++)
			{
				HtmlElement element = _elements[i];
				if (element.type == HtmlElementType.LinkStart)
				{
					if (quadIndex >= element.quadStart && quadIndex < element.quadEnd)
						return element;
				}
			}

			return null;
		}

		internal IHtmlObject GetHtmlObject(string name)
		{
			int count = _elements.Count;
			for (int i = 0; i < count; i++)
			{
				HtmlElement element = _elements[i];
				if (element.htmlObject != null && name.Equals(element.name, StringComparison.OrdinalIgnoreCase))
					return element.htmlObject;
			}

			return null;
		}

		void OpenKeyboard()
		{
			_mobileInputAdapter.OpenKeyboard(_text, false, displayAsPassword ? false : !_singleLine, displayAsPassword, false, null);
		}

		void __focusIn(EventContext context)
		{
			if (_input)
			{
				if (_mobileInputAdapter != null)
				{
					OpenKeyboard();
				}
				else
				{
					_caret = Stage.inst.inputCaret;
					_caret.grahpics.sortingOrder = this.renderingOrder + 1;
					_caret.SetParent(cachedTransform);
					_caret.SetSizeAndColor(_textFormat.size, _textFormat.color);

					_highlighter = Stage.inst.highlighter;
					_highlighter.grahpics.sortingOrder = this.renderingOrder + 2;
					_highlighter.SetParent(cachedTransform);

					onKeyDown.AddCapture(__keydown);
					onTouchBegin.AddCapture(__touchBegin);
				}
			}
		}

		void __focusOut(EventContext contxt)
		{
			if (_mobileInputAdapter != null)
			{
				_mobileInputAdapter.CloseKeyboard();
			}

			if (_caret != null)
			{
				_caret.SetParent(null);
				_caret = null;
				_highlighter.SetParent(null);
				_highlighter = null;
				onKeyDown.RemoveCapture(__keydown);
				onTouchBegin.RemoveCapture(__touchBegin);
			}
		}

		void __keydown(EventContext context)
		{
			if (context.isDefaultPrevented)
				return;

			InputEvent evt = context.inputEvent;

			switch (evt.keyCode)
			{
				case KeyCode.Backspace:
					{
						context.PreventDefault();
						if (_selectionStart != null)
						{
							DeleteSelection();
							onChanged.Call();
						}
						else if (_caretPosition > 0)
						{
							int tmp = _caretPosition; //this.text 会修改_caretPosition
							_caretPosition--;
							this.text = _text.Substring(0, tmp - 1) + _text.Substring(tmp);
							onChanged.Call();
						}

						break;
					}

				case KeyCode.Delete:
					{
						context.PreventDefault();
						if (_selectionStart != null)
						{
							DeleteSelection();
							onChanged.Call();
						}
						else if (_caretPosition < _text.Length)
						{
							this.text = _text.Substring(0, _caretPosition) + _text.Substring(_caretPosition + 1);
							onChanged.Call();
						}

						break;
					}

				case KeyCode.LeftArrow:
					{
						context.PreventDefault();
						if (evt.shift)
						{
							if (_selectionStart == null)
								_selectionStart = GetCharPosition(_caretPosition);
						}
						else
							ClearSelection();
						if (_caretPosition > 0)
						{
							CharPosition cp = GetCharPosition(_caretPosition - 1);

							AdjustCaret(cp);
						}
						break;
					}

				case KeyCode.RightArrow:
					{
						context.PreventDefault();
						if (evt.shift)
						{
							if (_selectionStart == null)
								_selectionStart = GetCharPosition(_caretPosition);
						}
						else
							ClearSelection();
						if (_caretPosition < _text.Length)
						{
							CharPosition cp = GetCharPosition(_caretPosition + 1);
							AdjustCaret(cp);
						}
						break;
					}

				case KeyCode.UpArrow:
					{
						context.PreventDefault();
						if (evt.shift)
						{
							if (_selectionStart == null)
								_selectionStart = GetCharPosition(_caretPosition);
						}
						else
							ClearSelection();

						CharPosition cp = GetCharPosition(_caretPosition);
						if (cp.lineIndex == 0)
							return;

						LineInfo line = _lines[cp.lineIndex - 1];
						cp = GetCharPosition(new Vector3(_caret.cachedTransform.localPosition.x + _positionOffset.x, line.y, 0));
						AdjustCaret(cp);
						break;
					}


				case KeyCode.DownArrow:
					{
						context.PreventDefault();
						if (evt.shift)
						{
							if (_selectionStart == null)
								_selectionStart = GetCharPosition(_caretPosition);
						}
						else
							ClearSelection();

						CharPosition cp = GetCharPosition(_caretPosition);
						if (cp.lineIndex == _lines.Count - 1)
							return;

						LineInfo line = _lines[cp.lineIndex + 1];
						cp = GetCharPosition(new Vector3(_caret.cachedTransform.localPosition.x + this._positionOffset.x, line.y, 0));
						AdjustCaret(cp);
						break;
					}

				case KeyCode.PageUp:
					{
						context.PreventDefault();
						ClearSelection();

						break;
					}

				case KeyCode.PageDown:
					{
						context.PreventDefault();
						ClearSelection();

						break;
					}

				case KeyCode.Home:
					{
						context.PreventDefault();
						ClearSelection();

						CharPosition cp = GetCharPosition(_caretPosition);
						LineInfo line = _lines[cp.lineIndex];
						cp = GetCharPosition(new Vector3(int.MinValue, line.y, 0));
						AdjustCaret(cp);
						break;
					}

				case KeyCode.End:
					{
						context.PreventDefault();
						ClearSelection();

						CharPosition cp = GetCharPosition(_caretPosition);
						LineInfo line = _lines[cp.lineIndex];
						cp = GetCharPosition(new Vector3(int.MaxValue, line.y, 0));
						AdjustCaret(cp);

						break;
					}

				//Select All
				case KeyCode.A:
					{
						if (evt.ctrl)
						{
							context.PreventDefault();
							_selectionStart = GetCharPosition(0);
							AdjustCaret(GetCharPosition(_text.Length));
						}
						break;
					}

				// Copy
				case KeyCode.C:
					{
						if (evt.ctrl && !_displayAsPassword)
						{
							context.PreventDefault();
							string s = GetSelection();
							if (!string.IsNullOrEmpty(s))
								Stage.inst.onCopy.Call(s);
						}
						break;
					}

				// Paste
				case KeyCode.V:
					{
						if (evt.ctrl)
						{
							context.PreventDefault();
							Stage.inst.onPaste.Call(this);
						}
						break;
					}

				// Cut
				case KeyCode.X:
					{
						if (evt.ctrl && !_displayAsPassword)
						{
							context.PreventDefault();
							string s = GetSelection();
							if (!string.IsNullOrEmpty(s))
							{
								Stage.inst.onCopy.Call(s);
								DeleteSelection();
								onChanged.Call();
							}
						}
						break;
					}

				case KeyCode.Return:
				case KeyCode.KeypadEnter:
					{
						if (!evt.ctrl && !evt.shift)
						{
							context.PreventDefault();

							if (!_singleLine)
								InsertText("\n");
						}
						break;
					}
			}
		}

		void __touchBegin(EventContext context)
		{
			if (_lines.Count == 0)
				return;

			ClearSelection();

			CharPosition cp;
			if (_textChanged) //maybe the text changed in user's touchBegin
			{
				cp.charIndex = 0;
				cp.lineIndex = 0;
			}
			else
			{
				Vector3 v = Stage.inst.touchPosition;
				v = this.GlobalToLocal(v);
				v.x += this._positionOffset.x;
				v.y += this._positionOffset.y;
				cp = GetCharPosition(v);
			}

			AdjustCaret(cp);
			_selectionStart = cp;
			Stage.inst.onTouchMove.AddCapture(__touchMove);
			Stage.inst.onTouchEnd.AddCapture(__touchEnd);
		}

		void __touchMove(EventContext context)
		{
			if (_selectionStart == null)
				return;

			Vector3 v = Stage.inst.touchPosition;
			v = this.GlobalToLocal(v);
			if (float.IsNaN(v.x))
				return;

			v.x += this._positionOffset.x;
			v.y += this._positionOffset.y;
			CharPosition cp = GetCharPosition(v);
			if (cp.charIndex != _caretPosition)
				AdjustCaret(cp);
		}

		void __touchEnd(EventContext context)
		{
			if (_selectionStart != null && ((CharPosition)_selectionStart).charIndex == _caretPosition)
				_selectionStart = null;
			Stage.inst.onTouchMove.RemoveCapture(__touchMove);
			Stage.inst.onTouchEnd.RemoveCapture(__touchEnd);
		}

		class LineInfo
		{
			public float width;
			public float height;
			public float textHeight;
			public string text;
			public float y;
			public int quadCount;

			static Stack<LineInfo> pool = new Stack<LineInfo>();

			public static LineInfo Borrow()
			{
				if (pool.Count > 0)
				{
					LineInfo ret = pool.Pop();
					ret.width = 0;
					ret.height = 0;
					ret.textHeight = 0;
					ret.text = null;
					ret.y = 0;
					ret.quadCount = 0;
					return ret;
				}
				else
					return new LineInfo();
			}

			public static void Return(LineInfo value)
			{
				pool.Push(value);
			}

			public static void Return(List<LineInfo> values)
			{
				int cnt = values.Count;
				for (int i = 0; i < cnt; i++)
					pool.Push(values[i]);

				values.Clear();
			}
		}

		struct CharPosition
		{
			public int charIndex;
			public int lineIndex;
		}
	}

}
