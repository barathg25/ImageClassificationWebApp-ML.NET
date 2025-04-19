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

        // Called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // Determine whether user consent for non-essential cookies is needed.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // Add controllers and views
            services.AddControllersWithViews();

            // Register ML.NET prediction engine pool
            services.AddPredictionEnginePool<InMemoryImageData, ImagePrediction>()
                    .FromFile(Configuration["MLModel:MLModelFilePath"]);
        }

        // Called by the runtime. Use this method to configure the HTTP request pipeline.
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

            app.UseDefaultFiles();      // ✅ Serve index.html by default from wwwroot
            app.UseStaticFiles();       // ✅ Allow serving of static files like HTML, CSS, JS
            app.UseCookiePolicy();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();  // ✅ Map API controllers (e.g., /Predict)
            });
        }

        // (Optional) Utility to warm up prediction engine
        public static void WarmUpPredictionEnginePool(IServiceCollection services)
        {
            var predictionEnginePool = services.BuildServiceProvider()
                .GetRequiredService<PredictionEnginePool<InMemoryImageData, ImagePrediction>>();
            var predictionEngine = predictionEnginePool.GetPredictionEngine();
            predictionEnginePool.ReturnPredictionEngine(predictionEngine);
        }

        // (Optional) Utility to get full path to a model file
        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;
            return Path.Combine(assemblyFolderPath, relativePath);
        }
    }
}
