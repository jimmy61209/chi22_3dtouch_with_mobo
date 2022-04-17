/************************************************/
/*                                              */
/*     Copyright (c) 2018 - 2021 monitor1394    */
/*     https://github.com/monitor1394           */
/*                                              */
/************************************************/

using UnityEngine;
using System;

namespace XCharts
{
    [AddComponentMenu("XCharts/ScatterChart", 17)]
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class ScatterChart : CoordinateChart
    {
        private float m_EffectScatterSpeed = 15;
        private float m_EffectScatterSize;
        private float m_EffectScatterAplha;
        public int clickedIndex = -1;
        public Action<int, bool, string> action;







#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            title.text = "ScatterChart";
            tooltip.type = Tooltip.Type.None;
            m_XAxes[0].type = Axis.AxisType.Value;
            m_XAxes[0].boundaryGap = false;
            m_YAxes[1].type = Axis.AxisType.Value;
            m_XAxes[1].boundaryGap = false;
            RemoveData();
            SerieTemplate.AddDefaultScatterSerie(this, "serie1");
        }
#endif

        protected override void Update()
        {
            base.Update();
            bool hasEffectScatter = false;
            foreach (var serie in m_Series.list)
            {
                if (serie.type == SerieType.EffectScatter)
                {
                    hasEffectScatter = true;
                    var symbolSize = serie.symbol.GetSize(null, m_Theme.serie.scatterSymbolSize);
                    for (int i = 0; i < serie.symbol.animationSize.Count; ++i)
                    {
                        serie.symbol.animationSize[i] += m_EffectScatterSpeed * Time.deltaTime;
                        if (serie.symbol.animationSize[i] > symbolSize)
                        {
                            serie.symbol.animationSize[i] = i * 5;
                        }
                    }
                }
            }
            if (hasEffectScatter)
            {
                RefreshChart();
            }
        }

        protected override void CheckTootipArea(Vector2 local, bool isActivedOther)
        {
            // local = new Vector2(Screen.width / 2, Screen.height / 2);
            // print(local);
            // local += new Vector2(Input.mousePosition.x / 10, Input.mousePosition.y / 10);
            // print(Input.mousePosition);

            base.CheckTootipArea(local, isActivedOther);
            if (isActivedOther) return;
            tooltip.ClearSerieDataIndex();
            bool selected = false;
            foreach (var serie in m_Series.list)
            {
                if (!serie.show) continue;
                if (serie.type != SerieType.Scatter && serie.type != SerieType.EffectScatter) continue;
                bool refresh = false;
                var dataCount = serie.data.Count;
                for (int j = 0; j < serie.data.Count; j++)
                {
                    var serieData = serie.data[j];
                    var symbol = SerieHelper.GetSerieSymbol(serie, serieData);
                    if (!symbol.ShowSymbol(j, dataCount)) continue;
                    var dist = Vector3.Distance(local, serieData.runtimePosition);
                    if (dist <= symbol.GetSize(serieData.data, m_Theme.serie.scatterSymbolSize))
                    {
                        serieData.selected = true;
                        tooltip.AddSerieDataIndex(serie.index, j);
                        selected = true;
                        // transform.GetComponent<ScatterController>()
                        if (action != null)
                        {
                            action(j, true, serie.name);

                        }
                    }
                    else
                    {
                        serieData.selected = false;

                        if (action != null)
                        {
                            action(j, false, serie.name);

                        }
                    }
                }
                if (refresh) RefreshChart();
            }



            if (selected)
            {
                tooltip.UpdateContentPos(local + tooltip.offset);
                UpdateTooltip();
            }
            else if (tooltip.IsActive())
            {
                tooltip.SetActive(false);
                RefreshChart();
            }
        }

        protected override void UpdateTooltip()
        {
            // if(clickedIndex != -1) return;
            base.UpdateTooltip();
            if (tooltip.isAnySerieDataIndex())
            {
                var content = TooltipHelper.GetFormatterContent(tooltip, 0, this);
                int redlineIndex = content.IndexOf("<color=#FF1000FF>") - 1;
                if (redlineIndex != -2)
                {
                    content = content.Substring(0, redlineIndex);
                }
                int emptyCircleIndex = content.IndexOf("<color=#26A1DBFF>") - 1;
                if (emptyCircleIndex != -2)
                {
                    content = content.Substring(0, emptyCircleIndex);
                }
                content = content.Replace("FF0000FF", "26A1DBFF");


                int colorIndex = content.IndexOf(">● </color>") + 11;
                content = content.Substring(colorIndex, content.Length - colorIndex);

                TooltipHelper.SetContentAndPosition(tooltip, content, chartRect);
                tooltip.SetActive(true);
            }
            else
            {
                tooltip.SetActive(false);
            }
        }
    }
}
