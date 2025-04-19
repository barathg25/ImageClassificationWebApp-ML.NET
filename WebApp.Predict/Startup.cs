using ImageClassification.DataModels;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ML;
using System.IO;

namespace ImageClassification.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Use AddControllers() for API controller support
            services.AddControllersWithViews();

            /////////////////////////////////////////////////////////////////////////////
            // Register the PredictionEnginePool as a service in the IoC container for DI.
            //
            services.AddPredictionEnginePool<InMemoryImageData, ImagePrediction>()
                    .FromFile(Configuration["MLModel:MLModelFilePath"]);

            // (Optional) Get the pool to initialize it and warm it up.
            // WarmUpPredictionEnginePool(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            // Use Endpoint routing with MapControllers().
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Add support for controllers
                endpoints.MapControllers();
            });
        }

        public static void WarmUpPredictionEnginePool(IServiceCollection services)
        {
            // #1 - Simply get a Prediction Engine
            var predictionEnginePool = services.BuildServiceProvider().GetRequiredService<PredictionEnginePool<InMemoryImageData, ImagePrediction>>();
            var predictionEngine = predictionEnginePool.GetPredictionEngine();
            predictionEnginePool.ReturnPredictionEngine(predictionEngine);

            // #2 - Predict (optional)
            // Uncomment and implement image prediction as needed.
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);
            return fullPath;
        }
    }
}
