using UnityEngine;
using Verse;

namespace CentralizedThermalDistribution
{
    // We want the pipe graphic to behave like Graphic_Linked, however we need to override ShouldLinkWith() to handle color connections properly.
    // We could use the custom linkFlags, but with 4 pipe colors that would need 4 of the very limited custom flags which could break other mods.
    // Normally, Things that have linked graphics start as Graphic_Single in the Def, then later get transformed into Graphic_Linked or a subclass based on the linkType.
    // Unfortunately, the code that does this transformation is internal to the engine, and linkTypes are hardcoded.
    // Additionally, a Thing's Graphic cannot easily be changed once processed from the Def and initialized by GraphicDatabase.
    // Thus, we need a custom Graphic class that behives like Graphic_Single during init and elsewhere so it can be used in the Def, but behaves like Graphic_Linked during rendering.
    // I don't want to copy decompiled code either. I want to use inheiritance and override only what we need. Let the base code handle everything else.

    //Step 1: Define a Graphic_Linked subclass that has the modified ShouldLinkWith method.
    public class Graphic_Linked_CoolantPipe : Graphic_Linked
    {
        public CompCoolant.PipeColor pipeColor = CompCoolant.PipeColor.None; // Required by ShouldLinkWith

        public Graphic_Linked_CoolantPipe(Graphic subGraphic) : base(subGraphic) { } // Using base constructor
        public Graphic_Linked_CoolantPipe(Graphic subGraphic, CompCoolant.PipeColor color) : base(subGraphic) { pipeColor = color; } // Used for overlays

        public override bool ShouldLinkWith(IntVec3 vec, Thing parent) // Our modified method. The entire point of this crazy mess.
        {
            if (pipeColor == CompCoolant.PipeColor.Trader)
            {
                return parent.OccupiedRect().Contains(vec); // If trader (building), only connect to other cells within the same building.
            }
            else
            {
                return vec.InBounds(parent.Map) && parent.Map.GetComponent<CoolantNetManager>().IsPipeAt(vec, pipeColor);
            }
        }
    }

    // Step 2: Define a Graphic wrapper subclass that will be used in the Def.
    public class Graphic_Wrapper_CoolantPipe : Graphic_Single
    {
        private Graphic_Single subGraphicSingle = null; // Wrapped Graphic_Single instance
        private Graphic_Linked_CoolantPipe subGraphicLinked = null; //Wrapped Graphic_Linked instance

        public Graphic_Wrapper_CoolantPipe() : base() { } // Using base constructor

        public CompCoolant.PipeColor pipeColor // Allow subGraphicLinked's pipe color to be set. 
        {
            get => subGraphicLinked.pipeColor;
            set => subGraphicLinked.pipeColor = value;
        }

        // Step 3: Override the Init method to intialize both this wrapper instance and the subGraphicSingle instance to be identical. This allows GraphicDatabase to init it properly.
        // We could probably get away with making this wrapper class only inheirit from Graphic and only Init the subGraphicSingle class, but duplicating the data like this covers more bases.
        // Initially I tried without a subGraphicSingle instance and just used "new Graphic_Linked_CoolantPipe(this)", however that results in infinite method recursion.
        public override void Init(GraphicRequest req)
        {
            base.Init(req);
            subGraphicSingle = new Graphic_Single();
            subGraphicSingle.Init(req);
            subGraphicLinked = new Graphic_Linked_CoolantPipe(subGraphicSingle);
        }

        // Step 4: Forward all Graphic_Linked's overrides to the wrapped Graphic_Linked_CoolantPipe instance. This makes it act like Graphic_Linked during drawing.
        public override Material MatSingle => subGraphicLinked.MatSingle;
        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo) => subGraphicLinked.GetColoredVersion(newShader, newColor, newColorTwo);
        public override Material MatSingleFor(Thing thing) => subGraphicLinked.MatSingleFor(thing);
        public override void Print(SectionLayer layer, Thing thing, float extraRotation) => subGraphicLinked.Print(layer, thing, extraRotation);
    }
}