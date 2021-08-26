using Assets.Generation.G;
using Assets.Generation.GeomRep;
using Assets.Generation.GeomRep.Layouts;
using Assets.Generation.U;
using UnityEngine;

namespace Assets.Generation.Templates
{
    public class TemplateStore1 : TemplateStore
    {
        public TemplateStore1()
        {                      
            CorridorLayout.RegisterCustom("standard",
                new CorridorLayout.LayerData("floor", 0.8f),
                new CorridorLayout.LayerData("wall", 1)
            );

            CorridorLayout.RegisterCustom("fire_grass",
                new CorridorLayout.LayerData("floor", 1.4f),
                new CorridorLayout.LayerData("fire", 0.4f),
                new CorridorLayout.LayerData("grass", 1)
            );

            CorridorLayout.RegisterCustom("walled_fire",
                new CorridorLayout.LayerData("wall", 1),
                new CorridorLayout.LayerData("fire", 0.7f),
                new CorridorLayout.LayerData("floor", 0.8f)
            );

            {
                TemplateBuilder tb = new TemplateBuilder("Extend Corridor", "e");
                tb.AddNode(NodeRecord.NodeType.In, "i");
                tb.AddNode(NodeRecord.NodeType.Out, "o");
                tb.AddNode(NodeRecord.NodeType.Internal, "e2", false, "<target>", "o", null, "e", CircularGeomLayout.Instance);
                tb.AddNode(NodeRecord.NodeType.Internal, "e1", false, "<target>", "i", null, "e", CircularGeomLayout.Instance);

                tb.Connect("i", "e1", 4.5f, -1, null, -1);
                tb.Connect("e1", "e2", 4.5f, -1, null, -1);
                tb.Connect("e2", "o", 4.5f, -1, null, -1);

                AddTemplate(tb.Build());
            }

            //{
            //    TemplateBuilder tb = new TemplateBuilder("Split Corridor", "e");
            //    tb.AddNode(NodeRecord.NodeType.In, "i");
            //    tb.AddNode(NodeRecord.NodeType.Out, "o");
            //    tb.AddNode(NodeRecord.NodeType.Internal, "e1", true, "<target>", "i", null, "e", 1, CircularGeomLayout.Instance);
            //    tb.AddNode(NodeRecord.NodeType.Internal, "e2", true, "<target>", "i", null, "e", 0.5f, CircularGeomLayout.Instance);

            //    tb.Connect("i", "e1", 4.5f, -1, CorridorLayout.Instance);
            //    tb.Connect("e1", "o", 4.5f, -1, CorridorLayout.Instance);
            //    tb.Connect("i", "e2", 4.5f, 0.5f, CorridorLayout.Instance);
            //    tb.Connect("e2", "o", 4.5f, 0.5f, CorridorLayout.Instance);

            //    AddTemplate(tb.Build());
            //}

            //{
            //    TemplateBuilder tb = new TemplateBuilder("Extend Dead-end", "e");
            //    tb.AddNode(NodeRecord.NodeType.In, "i");
            //    tb.AddNode(NodeRecord.NodeType.Internal, "e1", false, "<target>", "i", null, "e", 1f, CircularGeomLayout.Instance);
            //    tb.AddNode(NodeRecord.NodeType.Internal, "e2", false, "<target>", null, "i", "e", 2f, CircularGeomLayout.Instance);

            //    tb.Connect("i", "e1", 4.5f, -1, CorridorLayout.Instance);
            //    tb.Connect("e1", "e2", 4.5f, -1, CorridorLayout.Instance);

            //    AddTemplate(tb.Build());
            //}

            {
                TemplateBuilder tb = new TemplateBuilder("Tee", "e");
                tb.AddNode(NodeRecord.NodeType.In, "i");
                tb.AddNode(NodeRecord.NodeType.Out, "o");
                tb.AddNode(NodeRecord.NodeType.Internal, "j", false, "<target>", null, null, "je", CircularGeomLayout.Instance);
                tb.AddNode(NodeRecord.NodeType.Internal, "side", true, "<target>", null, null, "e", 2f, 0.2f, CircularGeomLayout.Instance);

                tb.Connect("i", "j", 4.5f, -1, null, -1);
                tb.Connect("j", "o", 4.5f, -1, null, -1);
                tb.Connect("j", "side", 4.5f, 0.5f, CorridorLayout.Custom("walled_fire"), 0.2f);

                AddTemplate(tb.Build());
            }

            //{
            //    TemplateBuilder tb = new TemplateBuilder("Loop Tee", "");
            //    tb.AddNode(NodeRecord.NodeType.In, "i");
            //    tb.AddNode(NodeRecord.NodeType.Out, "o");
            //    tb.AddNode(NodeRecord.NodeType.Internal, "j", false, "<target>", null, null, "", 1f, CircularGeomLayout.Instance);
            //    tb.AddNode(NodeRecord.NodeType.Internal, "side-j", true, "<target>", null, "i", "", 1f, CircularGeomLayout.Instance);
            //    tb.AddNode(NodeRecord.NodeType.Internal, "side", true, "<target>", null, "o", "", 4f, CircularGeomLayout.Instance);

            //    tb.Connect("i", "j", 4.5f, -1, CorridorLayout.Instance);
            //    tb.Connect("j", "o", 4.5f, -1, CorridorLayout.Instance);
            //    tb.Connect("j", "side-j", 4.5f, 0.5f, CorridorLayout.Instance);
            //    tb.Connect("side-j", "side", 4.5f, 0.5f, CorridorLayout.Instance);

            //    tb.ExtraForce("j", "side", 1, 1);
            //    tb.ExtraForce("j", "side-j", 15, 1);

            //    AddTemplate(tb.Build());
            //}

            //{
            //    TemplateBuilder tb = new TemplateBuilder("Triangle", "e");
            //    tb.AddNode(NodeRecord.NodeType.In, "i");
            //    tb.AddNode(NodeRecord.NodeType.Out, "o");
            //    tb.AddNode(NodeRecord.NodeType.Internal, "a", false, "<target>", null, null, "e", 1f, CircularGeomLayout.Instance);
            //    tb.AddNode(NodeRecord.NodeType.Internal, "b", true, "<target>", null, "i", "e", 1f, CircularGeomLayout.Instance);
            //    tb.AddNode(NodeRecord.NodeType.Internal, "c", true, "<target>", null, "o", "e", 1f, CircularGeomLayout.Instance);

            //    tb.Connect("i", "a", 4.5f, -1, CorridorLayout.Instance);
            //    tb.Connect("a", "o", 4.5f, -1, CorridorLayout.Instance);
            //    tb.Connect("a", "b", 4.5f, 0.3f, CorridorLayout.Instance);
            //    tb.Connect("b", "c", 4.5f, 0.3f, CorridorLayout.Instance);
            //    tb.Connect("a", "c", 4.5f, 0.3f, CorridorLayout.Instance);

            //    //tb.ExtraForce("a", "b", 10, 1);
            //    //tb.ExtraForce("b", "c", 10, 1);
            //    //tb.ExtraForce("c", "a", 10, 1);

            //    tb.ExtraClusterSeparation = 2;

            //    AddTemplate(tb.Build());
            //}

            {
                //DoorPostExpand dh = new DoorPostExpand();

                TemplateBuilder tb = new TemplateBuilder("Rotunda", ""/*, dh*/);
                tb.AddNode(NodeRecord.NodeType.In, "i");
                tb.AddNode(NodeRecord.NodeType.Internal, "rotunda", false, "<target>",
                      null, null, "", 4f, 0.5f,
                      FourCircularPillarsGeomLayout.Instance);

                tb.Connect("i", "rotunda", 4.5f, -1, null, -1);

                AddTemplate(tb.Build());
            }

            //      {
            //         engine.TemplateBuilder tb = new engine.TemplateBuilder("Space", "");
            //         tb.addNode(engine.NodeRecord.NodeType.In, "i");
            //         tb.addNode(engine.NodeRecord.NodeType.Out, "o");
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "j", false, "<target>", null, null, "", 55f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "void", true, "<target>", null, null, "", 200f, 0x80808080);
            //
            //         tb.connect("i", "j", 90, 110, 10);
            //         tb.connect("j", "o", 90, 110, 10);
            //         // just try a different distance
            //         tb.connect("j", "void", 140, 150, 10,  0x80808080);
            //
            //         AddTemplate(tb.Build());
            //      }

            //      {
            //         engine.TemplateBuilder tb = new engine.TemplateBuilder("Rotunda", "e");
            //         tb.addNode(engine.NodeRecord.NodeType.In, "i");
            //         tb.addNode(engine.NodeRecord.NodeType.Out, "o1");
            //         tb.addNode(engine.NodeRecord.NodeType.Out, "o2");
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "ji", false, "<target>", "i", null, "", 55f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "jo1", false, "<target>", "o1", null, "", 55f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "jo2", false, "<target>", "o2", null, "", 55f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "eio1", false, "ji", "jo1", null, "e", 20f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "eio2", false, "ji", "jo2", null, "e", 20f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "eo1o2", false, "jo2", "jo1", null, "e", 20f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "void", false, "<target>", null, null, "", 100f, 0x80808080);
            //
            //         tb.connect("i", "ji", 90, 110, 10);
            //         tb.connect("o1", "jo1", 90, 110, 10);
            //         tb.connect("o2", "jo2", 90, 110, 10);
            //         tb.connect("ji", "eio1", 90, 110, 10);
            //         tb.connect("eio1", "jo1", 90, 110, 10);
            //         tb.connect("jo1", "eo1o2", 90, 110, 10);
            //         tb.connect("eo1o2", "jo2", 90, 110, 10);
            //         tb.connect("jo2", "eio2", 90, 110, 10);
            //         tb.connect("eio2", "ji", 90, 110, 10);
            //
            //         AddTemplate(tb.Build());
            //      }

            //      {
            //         engine.TemplateBuilder tb = new engine.TemplateBuilder("Split", "e");
            //         tb.addNode(engine.NodeRecord.NodeType.In, "i");
            //         tb.addNode(engine.NodeRecord.NodeType.Out, "o");
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "e1", true, "<target>", null, null, "e", 55f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "e2", true, "<target>", null, null, "e", 55f);
            //
            //         tb.connect("i", "e1", 70, 90, 15);
            //         tb.connect("i", "e2", 70, 90, 15);
            //         tb.connect("e1", "o", 70, 90, 15);
            //         tb.connect("e2", "o", 70, 90, 15);
            //
            //         AddTemplate(tb.Build());
            //      }

            //{
            //    // DoorPostExpand dh = new DoorPostExpand();

            //    TemplateBuilder tb = new TemplateBuilder("Door", "e"/*, dh*/, DefaultLayoutFactory);
            //    tb.AddNode(NodeRecord.NodeType.In, "i");
            //    tb.AddNode(NodeRecord.NodeType.Out, "o");
            //    tb.AddNode(NodeRecord.NodeType.Internal, "j", false, "<target>", null, null, "e", 20f,
            //          0xff808040/*, a-> new CircularGeomLayout(a.getPos(), 10)*/);
            //    tb.AddNode(NodeRecord.NodeType.Internal, "e", true, "<target>", null, null, "e", 55f);
            //    tb.AddNode(NodeRecord.NodeType.Internal, "obstacle", true, "e", null, null, "", 55f);
            //    tb.AddNode(NodeRecord.NodeType.Internal, "key", true, "obstacle", null, null, "", 30f);
            //    tb.AddNode(NodeRecord.NodeType.Internal, "door", false, "<target>", "o", null, "", 30f);

            //    tb.Connect("i", "j", 120, 120, 10);
            //    tb.Connect("j", "e", 120, 120, 10);
            //    tb.Connect("e", "obstacle", 120, 120, 10);
            //    tb.Connect("obstacle", "key", 70, 90, 10);
            //    tb.Connect("j", "door", 120, 120, 10);
            //    tb.Connect("door", "o", 70, 90, 10);

            //    AddTemplate(tb.Build());
            //}

            //      {
            //         engine.TemplateBuilder tb = new engine.TemplateBuilder("Cluster", "e");
            //         tb.addNode(engine.NodeRecord.NodeType.In, "i");
            //         tb.addNode(engine.NodeRecord.NodeType.Out, "o");
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "a", false, "<target>", "i", null, "", 55f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "b", false, "<target>", "o", null, "", 55f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "c", true, "a", "b", null, "", 55f);
            //
            //         tb.connect("i", "a", 70, 90, 10);
            //         tb.connect("a", "b", 70, 90, 10);
            //         tb.connect("b", "o", 70, 90, 10);
            //         tb.connect("a", "c", 70, 90, 10);
            //         tb.connect("c", "b", 70, 90, 10);
            //
            //         AddTemplate(tb.Build());
            //      }

            //      {
            //         engine.TemplateBuilder tb = new engine.TemplateBuilder("EndLoop", "e");
            //         tb.addNode(engine.NodeRecord.NodeType.In, "i");
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "j", true, "<target>", null, null, "", 55f);
            //         // "-i" means in the opposite direction to "i" :-)
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "e1", true, "<target>", null, "i", "e", 100f);
            //         tb.addNode(engine.NodeRecord.NodeType.Internal, "e2", true, "<target>", null, "i", "e", 75f);
            //
            //         tb.connect("i", "j", 90, 110, 10);
            //         tb.connect("j", "e1", 90, 110, 10);
            //         tb.connect("j", "e2", 90, 110, 10);
            //         tb.connect("e1", "e2", 90, 110, 10);
            //
            //         AddTemplate(tb.Build());
            //      }
        }

        public override void MakeSeed(Graph g, ClRand clRand)
        {
            Node start = g.AddNode("Start", "<", 3f, 0.1f, CircularFireLakeGeomLayout.Instance);
            Node expander = g.AddNode("engine.StepperController", "e", 1f, CircularGeomLayout.Instance);
            Node end = g.AddNode("End", ">", 3f, 0.1f, CircularFireLakeGeomLayout.Instance);

            start.Position = new Vector2(0, -4);
            expander.Position = new Vector2(0, 0);
            end.Position = new Vector2(4, 0);

            g.Connect(start, expander, 4.5f, 1, CorridorLayout.Custom("fire_grass"));
            g.Connect(expander, end, 4.5f, 1, CorridorLayout.Custom("fire_grass"));
        }
    }
}