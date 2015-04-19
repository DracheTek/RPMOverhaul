using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace JSI
{
    class JSIHeadsUpDisplay : InternalModule
    {

        [KSPField]
        public string cameraTransform = string.Empty;
        [KSPField]
        public string backgroundColor = "0,0,0,0";
        private Color32 backgroundColorValue;


        private RasterPropMonitorComputer comp;//计算机系统实例
        private bool startupComplete;//是否启动完成的变量
        private List<InfoGauge> gaugeList = new List<InfoGauge>();


        public bool RenderHUD(RenderTexture screen, float cameraAspect)
        {
            if (screen == null || !startupComplete || HighLogic.LoadedSceneIsEditor)
                return false;
            //屏幕不存在，启动不完全，在编辑器内三种条件下直接返回。

            GL.Clear(true, true, backgroundColorValue);//擦除屏幕

            Vector3 coM = vessel.findWorldCenterOfMass();//质心位置
            //这里的vessel变量是从InternalModule提取的。
            Vector3 up = (coM - vessel.mainBody.position).normalized;//质心位置减去几何中心位置，求单位向量
            Vector3 forward = vessel.GetTransform().up;//求从正在控制的驾驶舱测得的前向速度
            Vector3 right = vessel.GetTransform().right;//求向右平移速度
            Vector3 top = Vector3.Cross(right, forward);//求前两者的叉积，得到向上的方向
            Vector3 north = Vector3.Exclude(up, (vessel.mainBody.position + (Vector3d)vessel.mainBody.transform.up * vessel.mainBody.Radius) - coM).normalized;

            Vector3d velocityVesselSurface = vessel.orbit.GetVel() - vessel.mainBody.getRFrmVel(coM);//轨道速度减去行星转速，得到地面速度
            Vector3 velocityVesselSurfaceUnit = velocityVesselSurface.normalized;//速度的方向

            float cosUp = Vector3.Dot(forward, up);//俯仰的余弦
            float cosRoll = Vector3.Dot(top, up);//滚转的余弦
            float sinRoll = Vector3.Dot(right, up);//滚转的正弦

            var normalizedRoll = new Vector2(cosRoll, sinRoll);//滚转角速度的单位向量。

            GL.PushMatrix();//保存运行前的状态。第一次运行显然为空




            foreach (InfoGauge gauge in gaugeList)//遍历信息条列表
            {
                GL.LoadPixelMatrix(-0.5f*screen.width, 0.5f*screen.width, 0.5f*screen.height, -0.5f*screen.height);//载入屏幕矩阵，准备渲染
                //原点在屏幕中心
                GL.Viewport(new Rect(0, 0, screen.width, screen.height));//载入视场
                float value = comp.ProcessVariable(gauge.Variable).MassageToFloat();//用来存读入的变量
                if (float.IsNaN(value))
                {
                    value = 0.0f;
                }

                if (gauge.Material != null)//读到信息条用的贴图
                {
                    float midPointCoord;//贴图中点坐标。所有信息条统一用原本俯仰条的渲染方法。
                    Vector2 usedTextureSize = new Vector2(gauge.TextureLimit.y - gauge.TextureLimit.x, gauge.TextureLimit.w - gauge.TextureLimit.z);
                    float textureOffset = usedTextureSize.x / gauge.Material.mainTexture.height;
                    if (gauge.Use360Horizon)//超过上限之后翻转
                    {
                        midPointCoord = JUtil.DualLerp(gauge.TextureLimit.z + 0.25f * usedTextureSize.y,
                            gauge.TextureLimit.w - 0.25f * usedTextureSize.y, gauge.Limit.x,
                            gauge.Limit.y, value);
                    }
                    else
                    {
                        midPointCoord = JUtil.DualLerp(gauge.TextureLimit.x, gauge.TextureLimit.y, gauge.Limit.x, gauge.Limit.y, value);
                    }
                    if (gauge.RotateWithVessel)//随载具滚转
                    {
                        gauge.Material.SetPass(0);
                        GL.Begin(GL.QUADS);

                        GL.TexCoord2(0.0f, midPointCoord + gauge.TextureSize);
                        GL.Vertex3((cosRoll * gauge.Position.x + sinRoll * gauge.Position.y), (-sinRoll * gauge.Position.x + cosRoll * gauge.Position.y), 0.0f);
                        GL.TexCoord2(1.0f, midPointCoord + gauge.TextureSize);
                        GL.Vertex3((cosRoll * (gauge.Position.x+gauge.Position.z) + sinRoll * gauge.Position.y), (-sinRoll * (gauge.Position.x+gauge.Position.z) + cosRoll * gauge.Position.y), 0.0f);
                        GL.TexCoord2(1.0f, midPointCoord - gauge.TextureSize);
                        GL.Vertex3((cosRoll * (gauge.Position.x + gauge.Position.z) + sinRoll * (gauge.Position.y+gauge.Position.w)), (-sinRoll * (gauge.Position.x+gauge.Position.z) + cosRoll * (gauge.Position.y+gauge.Position.w)), 0.0f);
                        GL.TexCoord2(0.0f, midPointCoord - gauge.TextureSize);
                        GL.Vertex3((cosRoll * (gauge.Position.x) + sinRoll * (gauge.Position.y + gauge.Position.w)) , (-sinRoll * (gauge.Position.x) + cosRoll * (gauge.Position.y + gauge.Position.w)), 0.0f);
                        GL.End();

                    }
                    else
                    {
                        gauge.Material.SetPass(0);
                        GL.Begin(GL.QUADS);
                        GL.TexCoord2(0.0f, midPointCoord + gauge.TextureSize);
                        GL.Vertex3(gauge.Position.x, gauge.Position.y, 0.0f);
                        GL.TexCoord2(1.0f, midPointCoord + gauge.TextureSize);
                        GL.Vertex3(gauge.Position.x + gauge.Position.z, gauge.Position.y, 0.0f);
                        GL.TexCoord2(1.0f, midPointCoord - gauge.TextureSize);
                        GL.Vertex3(gauge.Position.x + gauge.Position.z, gauge.Position.y + gauge.Position.w, 0.0f);
                        GL.TexCoord2(0.0f, midPointCoord - gauge.TextureSize);
                        GL.Vertex3(gauge.Position.x, gauge.Position.y + gauge.Position.w, 0.0f);
                        GL.End();
                    }

                }
                else
                {
                    JUtil.LogErrorMessage(this, "Material is null");
                }

            }
            GL.PopMatrix();//输出上述的矩阵
            return true;
        }

        public void Start()
        {
            if (HighLogic.LoadedSceneIsEditor)
                return;
            try
            {
                backgroundColorValue = ConfigNode.ParseColor32(backgroundColor);
                foreach (ConfigNode propNode in GameDatabase.Instance.GetConfigNodes("PROP"))
                {
                    ConfigNode[] moduleNodes = propNode.GetNodes("MODULE");

                    foreach(ConfigNode moduleNode in moduleNodes)
                    {
                        ConfigNode[] pageNodes = moduleNode.GetNodes("PAGE");

                        foreach(ConfigNode pagenode in pageNodes)
                        {
                            ConfigNode[] bghandlerNodes = pagenode.GetNodes("BACKGROUNDHANDLER");

                            foreach(ConfigNode bghandlerNode in bghandlerNodes)
                            {
                                ConfigNode[] gaugeNodes = bghandlerNode.GetNodes("GAUGE");
                                foreach(ConfigNode node in gaugeNodes)
                                {
                                    InfoGauge gauge = new InfoGauge();
                                    JUtil.LogErrorMessage(this, "one item created");
                                    gauge.Texture = node.GetValue("Texture");
                                    JUtil.LogErrorMessage(this, "texture obtained"); 
                                    gauge.Position = ConfigNode.ParseVector4(node.GetValue("Position"));
                                    JUtil.LogErrorMessage(this, "position obtained");
                                    gauge.Limit = ConfigNode.ParseVector2(node.GetValue("Limit"));
                                    JUtil.LogErrorMessage(this, "limit obtained");
                                    gauge.TextureLimit = ConfigNode.ParseVector4(node.GetValue("TextureLimit"));
                                    JUtil.LogErrorMessage(this, "texlim obtained");
                                    gauge.TextureSize = float.Parse(node.GetValue("TextureSize"));
                                    JUtil.LogErrorMessage(this, "texsiz obtained");
                                    gauge.VerticalMovement = bool.Parse(node.GetValue("VerticalMovement"));
                                    JUtil.LogErrorMessage(this, "vertmov obtained");
                                    gauge.UseLog10 = bool.Parse(node.GetValue("UseLog10"));
                                    JUtil.LogErrorMessage(this, "uselog obtained");
                                    gauge.Use360Horizon = bool.Parse(node.GetValue("Use360Horizon"));
                                    JUtil.LogErrorMessage(this, "360 obtained");
                                    gauge.RotateWithVessel = bool.Parse(node.GetValue("RotateWithVessel"));
                                    JUtil.LogErrorMessage(this, "rotate obtained");
                                    gauge.Variable = node.GetValue("Variable");
                                    JUtil.LogErrorMessage(this, "variable obtained");
                                    gaugeList.Add(gauge);
                                    JUtil.LogErrorMessage(this, "one item added");
                                }

                            }

                        }
                    }
                }

                Shader unlit = Shader.Find("Hidden/Internal-GUITexture");
                foreach (InfoGauge gauge in gaugeList)
                {
                    if (!String.IsNullOrEmpty(gauge.Texture))
                    {
                        gauge.Material = new Material(unlit);
                        gauge.Material.color = new Color(0.0f,0.0f,0.0f,0.0f);
                        gauge.Material.mainTexture = GameDatabase.Instance.GetTexture(gauge.Texture.EnforceSlashes(), false);
                    }
                    if (gauge.Material.mainTexture != null)
                    {
                        float height = (float)gauge.Material.mainTexture.height;
                        float width = (float)gauge.Material.mainTexture.width;
                        gauge.TextureLimit.x = 1.0f - (gauge.TextureLimit.x / width);
                        gauge.TextureLimit.y = 1.0f - (gauge.TextureLimit.y / width);
                        gauge.TextureLimit.z = 1.0f - (gauge.TextureLimit.z / height);
                        gauge.TextureLimit.w = 1.0f - (gauge.TextureLimit.w / height);
                        gauge.TextureSize = 0.5f * (gauge.TextureSize/ height);//实际调用的时候用的是宽和高的一半。
                        if (gauge.Use360Horizon)
                        {
                            gauge.Material.mainTexture.wrapMode = TextureWrapMode.Repeat;
                        }
                        else
                        {
                            gauge.Material.mainTexture.wrapMode = TextureWrapMode.Clamp;
                        }

                    }
                    if (gauge.UseLog10)
                    {
                        gauge.Limit.x = JUtil.PseudoLog10(gauge.Limit.x);
                        gauge.Limit.y = JUtil.PseudoLog10(gauge.Limit.y);
                    }
                }
                JUtil.LogMessage(this, "material read");
                comp = RasterPropMonitorComputer.Instantiate(internalProp);
                startupComplete = true;
            }
            catch
            {
                JUtil.AnnoyUser(this);
                throw;
            }
        }
    }
}
