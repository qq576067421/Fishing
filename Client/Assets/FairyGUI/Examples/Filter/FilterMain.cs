using UnityEngine;
using FairyGUI;
using DG.Tweening;

public class FilterMain : MonoBehaviour
{
	GComponent _mainView;
	GSlider _s0;
	GSlider _s1;
	GSlider _s2;
	GSlider _s3;


	void Awake()
	{
		Application.targetFrameRate = 60;
		Stage.inst.onKeyDown.Add(OnKeyDown);

		UIPackage.AddPackage("UI/Filter");
	}

	void Start()
	{
		_mainView = this.GetComponent<UIPanel>().ui;

		_s0 = _mainView.GetChild("s0").asSlider;
		_s1 = _mainView.GetChild("s1").asSlider;
		_s2 = _mainView.GetChild("s2").asSlider;
		_s3 = _mainView.GetChild("s3").asSlider;

		_s0.value = 100;
		_s1.value = 100;
		_s2.value = 100;
		_s3.value = 200;

		_s0.onChanged.Add(__sliderChanged);
		_s1.onChanged.Add(__sliderChanged);
		_s2.onChanged.Add(__sliderChanged);
		_s3.onChanged.Add(__sliderChanged);
	}

	void __sliderChanged(EventContext context)
	{
		int cnt = _mainView.numChildren;
		for (int i = 0; i < cnt; i++)
		{
			GObject obj = _mainView.GetChildAt(i);
			if (obj.filter is ColorFilter)
			{
				ColorFilter filter = (ColorFilter)obj.filter;
				filter.Reset();
				filter.AdjustBrightness((_s0.value - 100) / 100f);
				filter.AdjustContrast((_s1.value - 100) / 100f);
				filter.AdjustSaturation((_s2.value - 100) / 100f);
				filter.AdjustHue((_s3.value - 100) / 100f);
			}
		}
	}

	void OnKeyDown(EventContext context)
	{
		if (context.inputEvent.keyCode == KeyCode.Escape)
		{
			Application.Quit();
		}
	}
}