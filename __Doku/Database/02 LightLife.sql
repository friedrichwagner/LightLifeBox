drop view EXPORT_DELTATEST
drop view V_LLDELTATEST_EXPORT
create view V_LLDELTATEST_EXPORT
(
	ID,UserID,BoxID, Brightness0, CCT0, x0,y0, testmodeID, TestModeName,
    actStep,Brightness,cct,x,y,actduv,dBrightness,dCCT ,duv,frequency,Remark ,added
)
as
SELECT t.ID,t.UserID,t.BoxID, t.Brightness0, t.CCT0, t.x0,t.y0, t.testmode, m.Name,
       t.actStep,t.Brightness,t.cct,t.x,t.y,t.actduv,t.dBrightness,t.dCCT ,t.duv,t.frequency,t.Remark ,t.added
  FROM LLDeltaTest t, LLDeltaTestMode m
  where t.testmode = m.TestModeID
  
  
Create view V_LLDATA_EXPORT
		(	DataID, RoomID, RoomName, UserID,
			SequenceID, ActivationID, StepID,
			Brightness, CCT, duv, x, y, pimode, Remark, sender, receiver, MsgTypeID, MsgName, added
		)
as
select d.DataID, d.RoomID, r.Name, d.UserID,  
		d.SequenceID, d.ActivationID, d.StepID,
		d.Brightness, d.CCT, d.duv, d.x, d.y, d.pimode, d.Remark, d.sender, d.receiver, d.MsgTypeID, m.MsgName, d.added
from LLData d,  LLRoom r, llMsgType m
where d.Roomid = r.Roomid
and d.MsgTypeId = m.MsgID
GO

Create view V_LLTESTSEQUENCE_EXPORT
(
	SequenceID, SequenceDef, BoxID, UserID, Remark,
	PosID, CycleId, ActivationID,  StepID,  PILEDID, PILEDMode,  Brightness,  CCT,  duv,  x,  y,  RemarkPos,  added,  updated
)
as 
select  h.SequenceID, h.SequenceDef, h.BoxID, h.UserID, h.Remark,
		p.PosID, p.CycleId, p.ActivationID,  p.StepID,  p.PILEDID,   p1.PILEDMode, p.Brightness,  p.CCT,  p.duv,  p.x,  p.y,  p.Remark,  p.added,  p.updated
from LLTestSequenceHead h, LLTestSequencePos p, LLPiledMode p1
where h.SequenceID = p.SequenceID
and p.PILEDID = p1.PILEDID
