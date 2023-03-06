using SourceCopyTool;
using YamlDotNet.Serialization;

namespace SourceCopyTool {

    struct FConfig {
        public String source_path;
        public String destination_path;
        public String source_api_name;
        public String destination_api_name;
        public String copyright;
        public FConfig ( ) => (source_path, destination_path, source_api_name, destination_api_name, copyright) = (String.Empty, String.Empty, String.Empty, String.Empty, String.Empty);
    }

    struct FCopyFile {
        public FileInfo? file_info;
        public String contents;
        public FCopyFile ( ) => (file_info, contents) = (null, String.Empty);
        public FCopyFile ( FileInfo _file_info, String _contents ) => (file_info, contents) = (_file_info, _contents);
    }

}

internal class Program {
    static void Main ( string[] args ) {

        Int32 argc = args.Length;
        String yaml = String.Empty;

        if ( argc == 1 ) {
            if ( args[0].Equals ( "-h", StringComparison.OrdinalIgnoreCase ) || args[0].Equals ( "--help", StringComparison.OrdinalIgnoreCase ) ) {
                Console.WriteLine ( "Usage:\n\tSourceCopyTool.exe [setting file path]\n\t--init" );
                return;
            } else if ( args[0].Equals ( "--init", StringComparison.OrdinalIgnoreCase ) ) {
                // create setting files
                String contents =
@"source_path: ""Source_Path""
destination_path: ""Destination_Path""
source_api_name: ""Source_API""
destination_api_name: ""Destination_API""
copyright: ""// Copyright (c) 2020""
";
                File.WriteAllText ( "setting.yaml", contents );
                return;
            } else {
                String setting_file_path = args[argc - 1];
                if ( !File.Exists ( setting_file_path ) ) {
                    Console.WriteLine ( $"Setting file {setting_file_path} does not exist" );
                    return;
                }
                yaml = File.ReadAllText ( setting_file_path );
            }
        } else {
            Console.WriteLine ( $"파라미터:-h, --help, --init [{argc}]" );
            return;
        }

        var deserializer = new DeserializerBuilder ( ).Build ( );
        var config = deserializer.Deserialize<FConfig> ( yaml );

        config.source_path = config.source_path.Replace ( "/", "\\" );
        config.destination_path = config.destination_path.Replace ( "/", "\\" );

        // Check if source path is a directory
        if ( !Directory.Exists ( config.source_path ) ) {
            Console.WriteLine ( $"Source path {config.source_path} is not a directory" );
            return;
        }

        // Check if destination path is a directory
        if ( !Directory.Exists ( config.destination_path ) ) {
            Console.WriteLine ( $"Destination path {config.destination_path} is not a directory" );
            return;
        }

        var source_file_paths = Directory.GetFiles ( config.source_path, "*", SearchOption.AllDirectories )
                                .Where ( str => str.EndsWith ( ".h" ) || str.EndsWith ( ".cpp" ) );

        List<FCopyFile> copy_files = new ( );

        foreach ( var source_file_path in source_file_paths ) {
            String contents = String.Empty;
            if ( config.copyright.Length > 0 ) {
                contents += config.copyright + "\r\n\r\n";
            }
            foreach ( String str in File.ReadLines ( source_file_path, System.Text.Encoding.UTF8 ) ) {
                contents += str.Replace ( config.source_api_name, config.destination_api_name ) + "\r\n";
            }

            copy_files.Add ( new FCopyFile ( new FileInfo ( source_file_path ), contents ) );
        }

        var source_root_directory = Directory.GetDirectories ( config.source_path );

        // Create destination directory
        Directory.CreateDirectory ( config.destination_path );

        foreach ( var copy_file in copy_files ) {
            if ( copy_file.file_info == null ) {
                Console.WriteLine ( "copy file is null..." );
                continue;
            }
            var directory_path = copy_file.file_info.FullName;
            directory_path = directory_path.Replace ( config.source_path + "\\", "" );
            directory_path = directory_path.Replace ( copy_file.file_info.Name, "" );

            if( directory_path.Length > 0) {
                Directory.CreateDirectory ( Path.Combine ( config.destination_path, directory_path ) );
            }

            directory_path = Path.Combine( directory_path, copy_file.file_info.Name );
            String destination_file_path = Path.Combine ( config.destination_path, directory_path );

            if ( File.Exists ( destination_file_path ) ) {
                FileInfo fileInfo = new ( destination_file_path );
                if ( fileInfo.Equals ( copy_file.file_info ) ) {
                    continue;
                }
            } else {
                File.Copy ( copy_file.file_info.FullName, destination_file_path, true );
            }

            File.WriteAllText ( destination_file_path, copy_file.contents, System.Text.Encoding.UTF8 );
        }

    }
}
