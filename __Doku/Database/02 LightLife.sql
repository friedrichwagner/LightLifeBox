drop view EXPORT_DELTATEST
create view EXPORT_DELTATEST
(
	ID,UserID,BoxID, Brightness0, CCT0, x0,y0, testmodeID, TestModeName,
    actStep,Brightness,cct,x,y,actduv,dBrightness,dCCT ,duv,frequency,Remark ,added
)
as
SELECT t.ID,t.UserID,t.BoxID, t.Brightness0, t.CCT0, t.x0,t.y0, t.testmode, m.Name,
       t.actStep,t.Brightness,t.cct,t.x,t.y,t.actduv,t.dBrightness,t.dCCT ,t.duv,t.frequency,t.Remark ,t.added
  FROM LLDeltaTest t, LLDeltaTestMode m
  where t.testmode = m.TestModeID
  