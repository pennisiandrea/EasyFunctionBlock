namespace FilesContents
{
    public static class ExecuteFB
    {
        public const string MAIN_FILE_NAME = "__FBNAME__.st";
        public const string MAIN_FILE_CONTENT = 
        """
        FUNCTION_BLOCK __FBNAME__ 

            IF Execute AND NOT Internal.ExecuteOld THEN
                __FBNAME__CheckParameters;
                IF Internal.ParametersValid THEN
                    Internal.Executing := TRUE;
                END_IF;
            END_IF;
            
            Internal.ExecuteOld := Execute;
            
            IF Internal.Executing THEN
                __FBNAME__CyclicCode;
            END_IF;
                
            IF Done AND NOT Execute THEN
                __FBNAME__ResetInternal;
                __FBNAME__ResetOutputs;
            END_IF;
            
        END_FUNCTION_BLOCK
        """;

        public const string ACTIONS_FILE_NAME = "__FBNAME__Actions.st";
        public const string ACTIONS_FILE_CONTENT = 
        """
        ACTION __FBNAME__CheckParameters:
            IF  Parameter1 <= 0.0 THEN
                StatusID := 12345; (*add StatusID constant here*)
                __FBNAME__SetError;
            ELSIF  Parameter2 < 0.0 THEN
                StatusID := 54321; (*add StatusID constant here*)
                __FBNAME__SetError;
            ELSE
                Internal.Parameter1 := Parameter1;
                Internal.Parameter2 := Parameter2;
                Active := TRUE;
                Busy := TRUE;
                Error := FALSE;
                StatusID := 0;		
                Internal.ParametersValid := TRUE;	
                Internal.Executing := TRUE;
            END_IF;
        END_ACTION

        ACTION __FBNAME__SetError:
            Active := FALSE;
            Busy := TRUE;
            Done := FALSE;
            Error := TRUE;			
        END_ACTION

        ACTION __FBNAME__CyclicCode:
            (* add code here, when execution is complete call action FBname_SetDone *)
        END_ACTION

        ACTION __FBNAME__SetDone:
            Internal.Executing := FALSE;
            Done := TRUE;
            Busy := FALSE;
            Active := FALSE;			
        END_ACTION

        ACTION __FBNAME__ResetInternal:
            Internal.Parameter1 := 0.0;
            Internal.Parameter2 := 0.0;		
            Internal.ExecuteOld := FALSE;
            Internal.Executing := FALSE;
            Internal.ParametersValid := FALSE;	
        END_ACTION

        ACTION __FBNAME__ResetOutputs:
            Out := 0.0;
            Done := FALSE;
            Busy := FALSE;
            Active := FALSE;
            Error := FALSE;
            StatusID := 0;	
        END_ACTION
        """;

        public const string TYPES_FILE_NAME = "__FBNAME__Types.typ";
        public const string TYPES_FILE_CONTENT = 
        """
        TYPE
            __FBNAME__InternalType :     STRUCT  (*Template of a structure of internal parameters for an enable function block *) 
                ExecuteOld : BOOL; (*Variable to detect rising edge on Execute input*) 
                Executing : BOOL; (*Executing flag*) 
                ParametersValid : BOOL; (*All parameters valid flag *) 
                Parameter1 : REAL; (*Internal parameter required for computing*) 
                Parameter2 : REAL; (*Internal parameter required for computing*)          		
            END_STRUCT; 
        END_TYPE
        """;

        public const string FUNCTION_FILE_NAME = "__PKGNAME__.fun";
        public const string FUNCTION_FILE_CONTENT = 
        """
        FUNCTION_BLOCK __FBNAME__ (*FB template for Execute FB*)
            VAR_INPUT
                Execute : BOOL; (*Execute the function block*)
                Parameter1 : REAL; (*Parameter required for computing*) 
                Parameter2 : REAL; (*Parameter required for computing*) 
                In : REAL; (*Input variable*)
            END_VAR
            VAR_OUTPUT
                Done : BOOL; (*Execute is done*)
                Busy : BOOL; (*Function block is busy*)
                Active : BOOL; (*Function block is active*)
                Error : BOOL; (*Indicates an error*)
                StatusID : DINT; (*Status information*)
                Out : REAL; (*Output variable*)
            END_VAR
            VAR
                Internal : __FBNAME__InternalType; (*Data for internal use*)
            END_VAR
        END_FUNCTION_BLOCK
        """;

        public const string IEC_FILE_NAME = "IEC.*";
        public const string IEC_FILE_CONTENT = 
        """
        <?xml version="1.0" encoding="utf-8"?>
        <?AutomationStudio FileVersion="4.9"?>
        <Library SubType="IEC" xmlns="http://br-automation.co.at/AS/Library">
        <Files>
            <File Description="Exported data types">__FBNAME__Types.typ</File>
            <File Description="Exported functions and function blocks">__PKGNAME__.fun</File>
            <File>__FBNAME__.st</File>
            <File>__FBNAME__Actions.st</File>
        </Files>
        </Library>
        """;
        
    }
}