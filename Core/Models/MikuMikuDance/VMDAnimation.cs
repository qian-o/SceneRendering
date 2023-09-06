using Core.Models.MikuMikuDance.VMD;

namespace Core.Models.MikuMikuDance;

public class VMDAnimation
{
    private readonly List<VMDNodeController> nodeControllers = new();
    private readonly List<VMDMorphController> morphControllers = new();
    private readonly List<VMDIkController> ikControllers = new();

    private MMDModel? model;
    private uint maxKeyTime;

    public int MaxKeyTime => (int)maxKeyTime;

    public VMDAnimation()
    {

    }

    public bool Create(MMDModel mmd)
    {
        model = mmd;

        return true;
    }

    public bool Add(VMDFile vmd)
    {
        // Node Controller
        Dictionary<string, VMDNodeController> nodeCtrlMap = new();
        foreach (VMDNodeController nodeController in nodeControllers)
        {
            nodeCtrlMap.Add(nodeController.Node!.Name, nodeController);
        }
        nodeControllers.Clear();
        foreach (Motion motion in vmd.Motions)
        {
            string nodeName = motion.BoneName;
            if (!nodeCtrlMap.TryGetValue(nodeName, out VMDNodeController? nodeController))
            {
                if (model!.GetNodeManager()!.GetMMDNode(nodeName) is MMDNode node)
                {
                    nodeController = new VMDNodeController
                    {
                        Node = node
                    };
                    nodeCtrlMap.Add(nodeName, nodeController);
                }
            }

            if (nodeController != null)
            {
                VMDNodeAnimationKey key = new();
                key.Set(motion);

                nodeController.AddKey(key);
            }
        }
        foreach (KeyValuePair<string, VMDNodeController> pair in nodeCtrlMap)
        {
            pair.Value.SortKeys();
            nodeControllers.Add(pair.Value);
        }
        nodeCtrlMap.Clear();

        // Morph Controller
        Dictionary<string, VMDMorphController> morphCtrlMap = new();
        foreach (VMDMorphController morphController in morphControllers)
        {
            morphCtrlMap.Add(morphController.Morph!.Name, morphController);
        }
        morphControllers.Clear();
        foreach (Morph morph in vmd.Morphs)
        {
            string morphName = morph.BlendShapeName;
            if (!morphCtrlMap.TryGetValue(morphName, out VMDMorphController? morphController))
            {
                if (model!.GetMorphManager()!.GetMorph(morphName) is MMDMorph mmdMorph)
                {
                    morphController = new VMDMorphController
                    {
                        Morph = mmdMorph
                    };
                    morphCtrlMap.Add(morphName, morphController);
                }
            }

            if (morphController != null)
            {
                VMDMorphAnimationKey key = new()
                {
                    Time = (int)morph.Frame,
                    Weight = morph.Weight
                };

                morphController.AddKey(key);
            }
        }
        foreach (KeyValuePair<string, VMDMorphController> pair in morphCtrlMap)
        {
            pair.Value.SortKeys();
            morphControllers.Add(pair.Value);
        }
        morphCtrlMap.Clear();

        // IK Controller
        Dictionary<string, VMDIkController> ikCtrlMap = new();
        foreach (VMDIkController ikController in ikControllers)
        {
            ikCtrlMap.Add(ikController.IkSolver!.Name, ikController);
        }
        ikControllers.Clear();
        foreach (Ik ik in vmd.Iks)
        {
            foreach (IkInfo ikInfo in ik.IkInfos)
            {
                string ikName = ikInfo.Name;
                if (!ikCtrlMap.TryGetValue(ikName, out VMDIkController? ikController))
                {
                    if (model!.GetIkManager()!.GetMMDIkSolver(ikName) is MMDIkSolver ikSolver)
                    {
                        ikController = new VMDIkController
                        {
                            IkSolver = ikSolver
                        };
                        ikCtrlMap.Add(ikName, ikController);
                    }
                }

                if (ikController != null)
                {
                    VMDIKAnimationKey key = new()
                    {
                        Time = (int)ik.Frame,
                        Enable = ikInfo.Enable
                    };

                    ikController.AddKey(key);
                }
            }
        }
        foreach (KeyValuePair<string, VMDIkController> pair in ikCtrlMap)
        {
            pair.Value.SortKeys();
            ikControllers.Add(pair.Value);
        }
        ikCtrlMap.Clear();

        maxKeyTime = (uint)CalculateMaxKeyTime();

        return true;
    }

    public void Destroy()
    {
        nodeControllers.Clear();
        morphControllers.Clear();
        ikControllers.Clear();
        maxKeyTime = 0;
    }

    public void Evaluate(float t, float weight = 1.0f)
    {
        foreach (VMDNodeController nodeController in nodeControllers)
        {
            nodeController.Evaluate(t, weight);
        }

        foreach (VMDMorphController morphController in morphControllers)
        {
            morphController.Evaluate(t, weight);
        }

        foreach (VMDIkController ikController in ikControllers)
        {
            ikController.Evaluate(t, weight);
        }
    }

    /// <summary>
    /// Physics を同期させる
    /// </summary>
    /// <param name="t"></param>
    /// <param name="frameCount"></param>
    public void SyncPhysics(float t, int frameCount = 30)
    {
        /*
		すぐにアニメーションを反映すると、Physics が破たんする場合がある。
		例：足がスカートを突き破る等
		アニメーションを反映する際、初期状態から数フレームかけて、
		目的のポーズへ遷移させる。
		*/
        model!.SaveBaseAnimation();

        // Physicsを反映する
        for (int i = 0; i < frameCount; i++)
        {
            model.BeginAnimation();

            Evaluate(t, (1.0f + i) / frameCount);

            model.UpdateMorphAnimation();

            model.UpdateNodeAnimation(false);

            model.UpdatePhysicsAnimation(1.0f / 30.0f);

            model.UpdateNodeAnimation(true);

            model.EndAnimation();
        }
    }

    private int CalculateMaxKeyTime()
    {
        int maxTime = 0;

        foreach (VMDNodeController nodeController in nodeControllers)
        {
            if (nodeController.Keys.Any())
            {
                maxTime = Math.Max(maxTime, nodeController.Keys.Last().Time);
            }
        }

        foreach (VMDMorphController morphController in morphControllers)
        {
            if (morphController.Keys.Any())
            {
                maxTime = Math.Max(maxTime, morphController.Keys.Last().Time);
            }
        }

        foreach (VMDIkController ikController in ikControllers)
        {
            if (ikController.Keys.Any())
            {
                maxTime = Math.Max(maxTime, ikController.Keys.Last().Time);
            }
        }

        return maxTime;
    }
}
