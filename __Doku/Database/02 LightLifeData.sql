delete from LLTestSequenceState;
insert into LLTestSequenceState (StateID, Name) values (0, 'NONE');
insert into LLTestSequenceState (StateID, Name) values (10, 'IN_PROGRESS');
insert into LLTestSequenceState (StateID, Name) values (20, 'TESTING');
insert into LLTestSequenceState (StateID, Name) values (30, 'FADING_OUT');
insert into LLTestSequenceState (StateID, Name) values (80, 'PAUSED');
insert into LLTestSequenceState (StateID, Name) values (90, 'STOPPED');
insert into LLTestSequenceState (StateID, Name) values (99, 'FINISHED');



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
values ('JFBa', 'JFBa', 3,2,1,4,0,-1);