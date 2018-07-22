@echo off

SET fakeTool=packages\build\FAKE\tools\FAKE.exe
SET buildfsx=.buildtime/build.fsx 

%fakeTool% %buildfsx% %*