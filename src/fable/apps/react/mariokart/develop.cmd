@echo off

SET toRepoRoot=..\..\..\..\..\
SET fakeTool=%toRepoRoot%packages\build\FAKE\tools\FAKE.exe
SET buildfsx=%toRepoRoot%.buildtime/build.fsx 
SET developTarget=DevelopMk8React

%fakeTool% %buildfsx% %developTarget%