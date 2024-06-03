public void Main(string args)
{
    Echo(args);
    IMyTimerBlock Timer = GridTerminalSystem.GetBlockWithName(args) as IMyTimerBlock;
    if(Timer==null)
    {
        Echo("Non Fid Timer\n");
    }
    else
    {
        Timer.TriggerDelay = 0;
        Timer.Trigger();
    }
}