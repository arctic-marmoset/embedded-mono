#include <fmt/core.h>
#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/debug-helpers.h>
#include <mono/metadata/mono-debug.h>

#include <chrono>
#include <filesystem>
#include <string>
#include <thread>

#if defined(_WIN32) || defined(_WIN64)
#define GSS_API __declspec(dllexport)
#else
#define GSS_API
#endif

namespace fs = std::filesystem;
using namespace std::literals::chrono_literals;

static unsigned exit_code = 0xDEADBEEF;

namespace
{
    fs::path executable_absolute_path(int argc, const char *const argv[])
    {
        const fs::path path = argv[0];

        fs::path canonical_path = fs::weakly_canonical(path);
        return canonical_path;
    }
}

namespace managed
{
    class mono_context
    {
    public:
        static constexpr const char *domain_name = "gss::csharp";

        explicit mono_context(const fs::path &path, bool debug)
        {
            const fs::path lib_path = path / "lib";
            const fs::path etc_path = path / "etc";

            const std::string str_lib_path = lib_path.string();
            const std::string str_etc_path = etc_path.string();
            mono_set_dirs(lib_path.string().c_str(), str_etc_path.c_str());

            if (debug)
            {
                char soft_breakpoints[] = "--soft-breakpoints";
                char debugger_agent[] = "--debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555";
                char *argv[] = { soft_breakpoints, debugger_agent };
                constexpr int argc{ std::size(argv) };
                mono_jit_parse_options(argc, argv);
                mono_debug_init(MONO_DEBUG_FORMAT_MONO);
            }

            domain_ = mono_jit_init(domain_name);
            if (!domain_)
            {
                throw std::runtime_error("Failed to initialize Mono JIT");
            }

            // https://web.archive.org/web/20211106223141/https://bugzilla.xamarin.com/51/51261/bug.html#c2
            // We need to set a config to avoid encountering an "'ExeConfigFilename' argument cannot be null" error. It
            // doesn't matter whether the config actually exists. It just needs to be non-null.
            mono_domain_set_config(domain_, str_etc_path.c_str(), domain_name);
        }

        void init_scripts()
        {
            assembly_ = mono_domain_assembly_open(domain_, "managed/Interop.dll");
            if (!assembly_)
            {
                throw std::runtime_error("Failed to open assembly");
            }

            image_ = mono_assembly_get_image(assembly_);
            if (!image_)
            {
                throw std::runtime_error("Failed to get assembly image");
            }

            init_method_desc_ = mono_method_desc_new("Interop.Interop:Init()", true);
            if (!init_method_desc_)
            {
                fmt::print(stderr, "Failed to get init method desc");
            }

            update_method_desc_ = mono_method_desc_new("Interop.Interop:Update()", true);
            if (!update_method_desc_)
            {
                fmt::print(stderr, "Failed to get update method desc");
            }

            init_method_ = mono_method_desc_search_in_image(init_method_desc_, image_);
            update_method_ = mono_method_desc_search_in_image(update_method_desc_, image_);

            mono_runtime_invoke(init_method_, nullptr, nullptr, nullptr);
        }

        void update_scripts()
        {
            mono_runtime_invoke(update_method_, nullptr, nullptr, nullptr);
        }

        ~mono_context()
        {
            mono_jit_cleanup(domain_);
        }

    private:
        MonoDomain *domain_ = nullptr;
        MonoAssembly *assembly_ = nullptr;
        MonoImage *image_ = nullptr;
        MonoMethodDesc *init_method_desc_ = nullptr;
        MonoMethodDesc *update_method_desc_ = nullptr;
        MonoMethod *init_method_ = nullptr;
        MonoMethod *update_method_ = nullptr;
    };

    void entry(const fs::path &mono_path, bool debug)
    {
        mono_context context(mono_path, debug);

        context.init_scripts();

        for (int i = 0; i < 20; ++i)
        {
            context.update_scripts();
            std::this_thread::sleep_for(100ms);
        }
    }
}

extern "C" GSS_API void set_exit_code(unsigned code)
{
    exit_code = code;
}

int main(int argc, char *argv[])
{
    constexpr const char debug_flag[] = "--debug";

    const char *flag = argv[1];
    const bool debug =
        argc > 1 &&
        std::strcmp(flag, debug_flag) == 0;

    const fs::path executable_path = executable_absolute_path(argc, argv);
    fs::path mono_path = executable_path.parent_path() / MONO_RELATIVE_DIR;
    auto managed_worker = std::thread([path = std::move(mono_path), debug]
    {
        managed::entry(path, debug);
    });

    for (int i = 0; i < 20; ++i)
    {
        fmt::print("Rendering...\n");
        std::this_thread::sleep_for(100ms);
    }

    managed_worker.join();
    fmt::print("Exit code is: {:#x}\n", exit_code);
}
