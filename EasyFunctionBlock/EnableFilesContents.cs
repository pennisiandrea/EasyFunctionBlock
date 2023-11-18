namespace FilesContents
{
    public static class EnableFB
    {
        public const string MAIN_FILE_NAME = "__FBNAME__.st";
        public const string MAIN_FILE_CONTENT = 
        """
        FUNCTION_BLOCK __FBNAME__ 

            IF Enable THEN     
                IF NOT Internal.EnableOld THEN 
                    __FBNAME__CheckParameters;
                END_IF;

                IF Internal.ParametersValid THEN
                    __FBNAME__CyclicCode;
                END_IF;
                
                Internal.EnableOld := Enable;		
            ELSE
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
                Error := FALSE;
                StatusID := 0;		
                Internal.ParametersValid := TRUE;	
            END_IF;
        END_ACTION

        ACTION __FBNAME__SetError:
            Active := FALSE;
            Error := TRUE;			
        END_ACTION

        ACTION __FBNAME__CyclicCode:
            (*add main functionality of FB here*)	
        END_ACTION
            
        ACTION __FBNAME__ResetInternal:
            Internal.Parameter1 := 0.0;
            Internal.Parameter2 := 0.0;		
            Internal.EnableOld := FALSE;
            Internal.ParametersValid := FALSE;	
        END_ACTION

        ACTION __FBNAME__ResetOutputs:
            Out := 0.0;
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
                EnableOld : BOOL; (*Variable to detect rising edge on Enable input*) 
                ParametersValid : BOOL; (*All parameters valid flag*) 
                Parameter1 : REAL; (*Internal parameter for computing*)
                Parameter2 : REAL; (*Internal parameter for computing*) 
            END_STRUCT; 
        END_TYPE
        """;

        public const string FUNCTION_FILE_NAME = "__PKGNAME__.fun";
        public const string FUNCTION_FILE_CONTENT = 
        """
        FUNCTION_BLOCK __FBNAME__ (*FB template for Enable FB*) 
            VAR_INPUT 
                Enable : BOOL; (*Enable the function block*) 
                Parameter1 : REAL; (*Parameter required for computing*) 
                Parameter2 : REAL; (*Parameter required for computing*) 
                In : REAL; (*Input variable*) 
            END_VAR 
            VAR_OUTPUT 
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
        <Objects>
            <Object Type="File" Description="Exported functions and function blocks">__PKGNAME__.fun</Object>
            <Object Type="File" Description="Exported data types">__FBNAME__Types.typ</Object>
            <Object Type="Package">__FBNAME__</Object>
        </Objects>
        </Library>
        """;
        
        public const string SUBPKG_FILE_NAME = "Package.pkg";
        public const string SUBPKG_FILE_CONTENT = 
        """        
        <?xml version="1.0" encoding="utf-8"?>
        <?AutomationStudio FileVersion="4.9"?>
        <Package xmlns="http://br-automation.co.at/AS/Package">
        <Objects>
            <Object Type="File">__FBNAME__.st</Object>
            <Object Type="File">__FBNAME__Actions.st</Object>
        </Objects>
        </Package>
        """;
        
    }
}