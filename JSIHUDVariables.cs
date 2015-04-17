using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JSI
{
    public class InfoGauge : InternalModule
    {
        public readonly int gaugeID;
        [KSPField] // 贴图
        public string Texture = string.Empty;
        [KSPField] // 绘图区的位置（从左上角开始），绘图区的大小（横，纵）
        public Vector4 Position = new Vector4(0f, 0f, 64f, 320f);
        [KSPField] // 输入数据的上下限
        public Vector2 Limit = new Vector2(0f, 10000f);
        [KSPField] // 贴图绘制的限度（上，下，左，右）
        public Vector4 TextureLimit = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
        [KSPField] // 在绘图区绘制多少个像素（垂直缩放）
        public float TextureSize = 0.5f;
        [KSPField] // 调用的变量
        public string Variable = string.Empty;
        [KSPField] //垂直或水平
        public bool VerticalMovement = true;
        [KSPField] // 对数计数
        public bool UseLog10 = true;
        [KSPField] // 空翻一圈之后回到原位
        public bool Use360Horizon = true;
        [KSPField] //随载具滚转
        public bool RotateWithVessel = false;
        public Material Material;
        public static List<InfoGauge> gaugeList;

        //与信息条相关的变量

        [KSPField]
        public string cameraTransform = string.Empty;
        private FlyingCamera cameraObject;
        [KSPField]
        public string backgroundColor = "0,0,0,0";
        private Color32 backgroundColorValue;


        private RasterPropMonitorComputer comp;//计算机系统实例
        private bool startupComplete;//是否启动完成的变量


        public InfoGauge()
        {
            InfoGauge gauge = null;
            ConfigNode[] nodes = GameDatabase.Instance.GetConfigNodes("GAUGE");
            foreach (ConfigNode gaugeNode in nodes)
            {
                gauge.Texture = gaugeNode.GetValue("Texture");
                gauge.Position = ConfigNode.ParseVector4(gaugeNode.GetValue("Position"));
                gauge.Limit = ConfigNode.ParseVector2(gaugeNode.GetValue("Limit"));
                gauge.TextureLimit = ConfigNode.ParseVector2(gaugeNode.GetValue("TextureLimit"));
                gauge.TextureSize = float.Parse(gaugeNode.GetValue("TextureSize"));
                gauge.Variable = gaugeNode.GetValue("Variable");
                gauge.VerticalMovement = bool.Parse(gaugeNode.GetValue("VerticalMovement"));
                gauge.UseLog10 = bool.Parse(gaugeNode.GetValue("UseLog10"));
                gauge.Use360Horizon = bool.Parse(gaugeNode.GetValue("Use360horizon"));
                gauge.RotateWithVessel = bool.Parse(gaugeNode.GetValue("RotateWithVessel"));
                gaugeList.Add(gauge);
                JUtil.LogMessage(this, "Loading texture from URL, \"{0}\"", gaugeNode.GetValue("Texture"));
                JUtil.LogMessage(this, "position \"{0}", gaugeNode.GetValue("Position"));
            }
        }
    }
}
