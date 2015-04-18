using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JSI
{
    public class InfoGauge : InternalModule
    {
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

        //与信息条相关的变量

        public InfoGauge()
        {
            Texture = string.Empty;
            Position = new Vector4(0f, 0f, 64f, 320f);
            Limit = new Vector2(0f, 10000f);
            TextureLimit = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
            TextureSize = 0.5f;
            Variable = string.Empty;
            VerticalMovement = true;
            UseLog10 = true;
            Use360Horizon = true;
            RotateWithVessel = false;
            Material = null;
        }

        //构造函数
        public static InfoGauge ReadNode (ConfigNode node)
        {
            InfoGauge gauge = new InfoGauge();
                gauge.Texture = node.GetValue("Texture");
                gauge.Position = ConfigNode.ParseVector4(node.GetValue("Position"));
                gauge.Limit = ConfigNode.ParseVector2(node.GetValue("Limit"));
                gauge.TextureLimit = ConfigNode.ParseVector2(node.GetValue("TextureLimit"));
                gauge.TextureSize = float.Parse(node.GetValue("TextureSize"));
                gauge.Variable = node.GetValue("Variable");
                gauge.VerticalMovement = bool.Parse(node.GetValue("VerticalMovement"));
                gauge.UseLog10 = bool.Parse(node.GetValue("UseLog10"));
                gauge.Use360Horizon = bool.Parse(node.GetValue("Use360horizon"));
                gauge.RotateWithVessel = bool.Parse(node.GetValue("RotateWithVessel"));
                return gauge;
             
        }//这个函数从没用过 The Method is never used.
    }
}

