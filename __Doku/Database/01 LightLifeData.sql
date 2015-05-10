delete from LLTestSequenceDefinition;
insert into LLTestSequenceDefinition (SequenceDef, Name, StepID1, StepID2, StepID3, StepID4, StepID5, StepID6) 
values ('BFJa', 'BFJa', 1,2,3,4,0,-1);
insert into LLTestSequenceDefinition (SequenceDef, Name, StepID1, StepID2, StepID3, StepID4, StepID5, StepID6) 
values ('JBFa', 'JBFa', 3,1,2,4,0,-1);
insert into LLTestSequenceDefinition (SequenceDef, Name, StepID1, StepID2, StepID3, StepID4, StepID5, StepID6) 
values ('FJBa', 'FJBa', 2,3,1,4,0,-1);
insert into LLTestSequenceDefinition (SequenceDef, Name, StepID1, StepID2, StepID3, StepID4, StepID5, StepID6) 
values ('BJFa', 'BJFa', 1,3,2,4,0,-1);
insert into LLTestSequenceDefinition (SequenceDef, Name, StepID1, StepID2, StepID3, StepID4, StepID5, StepID6) 
values ('FBJa', 'FBJa', 2,1,3,4,0,-1);
insert into LLTestSequenceDefinition (SequenceDef, Name, StepID1, StepID2, StepID3, StepID4, StepID5, StepID6) 
values ('JFBa', 'FBJa', 3,2,1,4,0,-1);

---------

delete from LLMsgType;
insert into LLMsgType(MsgID, MsgName, Remark)
values (0, 'LL_NONE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (10, 'LL_SET_LIGHTS', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (20, 'LL_CALL_SCENE', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (50, 'LL_DISCOVER', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (51, 'LL_ENABLE_BUTTONS', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (52, 'LL_SET_PILED', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (53, 'LL_GET_PILED', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (54, 'LL_SET_SEQUENCEDATA', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (55, 'LL_START_DELTATEST', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (100, 'LL_SET_LOCKED', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (101, 'LL_SET_DEFAULT', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (102, 'LL_SET_LOCKED_DELTATEST', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (103, 'LL_AFTER_WAIT_TIME', null);

insert into LLMsgType(MsgID, MsgName, Remark)
values (104, 'LL_AFTER_FADE_TIME', null);

---------------
insert into LLDeltaTestMode (TestModeID, Name) values (0, 'NONE');
insert into LLDeltaTestMode (TestModeID, Name) values (1, 'BRIGHTNESS_UP');
insert into LLDeltaTestMode (TestModeID, Name) values (2, 'BRIGHTNESS_DOWN');
insert into LLDeltaTestMode (TestModeID, Name) values (3, 'CCT_UP');
insert into LLDeltaTestMode (TestModeID, Name) values (4, 'CCT_DOWN');
insert into LLDeltaTestMode (TestModeID, Name) values (5, 'JUDD_UP');
insert into LLDeltaTestMode (TestModeID, Name) values (6, 'JUDD_DOWN');




