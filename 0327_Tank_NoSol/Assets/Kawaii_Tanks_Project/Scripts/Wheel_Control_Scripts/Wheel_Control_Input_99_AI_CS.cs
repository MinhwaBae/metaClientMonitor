using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Wheel_Control_Input_99_AI_CS : Wheel_Control_Input_00_Base_CS
    {

        public override void Get_Input()
        {
            wheelControlScript.moveAxis.x = wheelControlScript.aiScript.turnOrder;
            wheelControlScript.moveAxis.y = wheelControlScript.aiScript.speedOrder;
        }

    }

}