using SourceCopyTool;
using YamlDotNet.Serialization;

namespace SourceCopyTool {

    struct Config {
        public String source_api_name;
        public String destination_api_name;
        public String copyright;
    }
    
}

internal class Program {
    static void Main(string[] args) {

        if ( args.Length < 3 ) {
            return;
        }

        Int32 argc = args.Length;

        String source_path = args[argc - 3];
        String destination_path = args[argc - 2];
        String setting_file_path = args[argc - 1];

        // Check if source path is a directory
        if ( !Directory.Exists(source_path) ) {
            Console.WriteLine($"Source path {source_path} is not a directory");
            return;
        }

        // Check if destination path is a directory
        if ( Directory.Exists(destination_path) ) {
            Console.WriteLine($"Destination path {destination_path} is not a directory");
            return;
        }

        if ( !File.Exists(setting_file_path) ) {
            Console.WriteLine($"Setting file {setting_file_path} does not exist");
            return;
        }

        String yaml = File.ReadAllText(setting_file_path);
        var deserializer = new DeserializerBuilder().Build();
        var config = deserializer.Deserialize<Config>(yaml);
        
        var source_file_paths = Directory.GetFiles(source_path);



        foreach ( var source_file_path in source_file_paths ) {
            String destination_file = String.Empty;
            if(config.copyright.Length > 0) {
                destination_file += config.copyright + "\r\n";
            }
            foreach ( String str in File.ReadLines(source_file_path) ) {
                destination_file += str.Replace(config.source_api_name, config.destination_api_name);
            }

        }

    }
}
