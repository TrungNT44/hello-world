using App.Common.MVC;
using System.Web.Optimization;

namespace MedMan.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            RegisterJsScripts(bundles);

            bundles.Add(new ScriptBundle("~/bundles/userpicker").Include(
                      "~/Scripts/userpicker.js"));
            bundles.Add(new ScriptBundle("~/bundles/nhathuocpicker").Include(
                      "~/Scripts/nhathuocpicker.js"));
            bundles.Add(new ScriptBundle("~/bundles/yesnodialog").Include(
                      "~/Scripts/yesnodialog.js"));
            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                    "~/Content/bootstrap.css",
                    "~/Content/bootstrap-responsive.css",
                    "~/Content/navbar.css"));
            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
              "~/Content/themes/base/core.css",
              "~/Content/themes/base/resizable.css",
              "~/Content/themes/base/selectable.css",
              "~/Content/themes/base/accordion.css",
              "~/Content/themes/base/autocomplete.css",
              "~/Content/themes/base/button.css",
              "~/Content/themes/base/dialog.css",
              "~/Content/themes/base/slider.css",
              "~/Content/themes/base/tabs.css",
              "~/Content/themes/base/datepicker.css",
              "~/Content/themes/base/progressbar.css",
              "~/Content/themes/base/theme.css",
              "~/Content/style.css",
              "~/Content/baocao.css"));
            RegisterCssScripts(bundles);

#if DEBUG

            BundleTable.EnableOptimizations = false;
            foreach (var bundle in BundleTable.Bundles)
            {
                bundle.Transforms.Clear();
            }
#else
            BundleTable.EnableOptimizations = true;
#endif
        }

        private static void RegisterJsScripts(BundleCollection bundles)
        {
            // Base
            bundles.Add(new GZipScriptBundle("~/bundles/base-lib-js")
                .Include(
                    "~/bower_components/jquery/jquery-2.1.3.min.js",
                    "~/bower_components/jquery/jquery-ui-1.11.2.min.js",
                    "~/bower_components/jquery/jquery.number.min.js",
                    "~/bower_components/jquery/jquery.cookie.js",
                    "~/bower_components/jquery/datepicker-vi.js",
                    "~/bower_components/bootstrap/dist/js/bootstrap.js",
                    "~/bower_components/angular/angular.min.js",
                    "~/bower_components/numeral-js/numeral.js",
                    "~/bower_components/numeral-js/angular-numeraljs.js",
                    "~/bower_components/angular-cache/angular-cache.js",
                    "~/bower_components/angular-bootstrap/ui-bootstrap.js",
                    "~/bower_components/angular-bootstrap/ui-bootstrap-tpls.js",
                    "~/bower_components/angular/ng-google-chart.js",
                    "~/bower_components/angular/angular-sanitize.js",
                    "~/bower_components/angular-route/angular-route.js",
                    "~/bower_components/tr-ng-grid/trNgGrid.js",
                    "~/bower_components/ag-grid/ag-grid.min.js",
                    "~/bower_components/utility/sprintf.min.js",
                    "~/bower_components/utility/date.js",
                    "~/bower_components/utility/string.js",
                    "~/bower_components/utility/mousetrap.min.js",
                    "~/bower_components/utility/mousetrap-global-bind.min.js",
                    "~/bower_components/utility/dynamic-number.min.js",
                    "~/bower_components/utility/angular-dateparser.js",
                    "~/bower_components/utility/socketProxy.js",
                    "~/bower_components/utility/math.min.js",
                    "~/bower_components/utility/loader.js",
                    "~/bower_components/utility/angular-object-diff.js",
                    "~/bower_components/moment/moment-with-locales.min.js",
                    "~/bower_components/loader/jquery.loading.min.js",
                    "~/bower_components/loader/loading.min.js",
                    "~/bower_components/bootbox/bootbox.js",
                    "~/bower_components/bootstrap-datepicker/js/bootstrap-datepicker.min.js",
                    "~/bower_components/bootstrap-datepicker/locales/bootstrap-datepicker.vi.min.js",
                    "~/bower_components/angular-number-input/angular-number-input.js",
                    "~/bower_components/selectize/selectize.js",
                    "~/bower_components/angular-ui-select/select.js",
                    "~/bower_components/select2/select2.min.js",
                    "~/bower_components/tableExport/excellentexport.js"
                ));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            // Production
            bundles.Add(new ScriptBundle("~/bundles/production-js")
                .Include(
                    "~/Scripts/setting.js",
                    "~/Scripts/app.js",
                    "~/Scripts/app-web-socket.js")
                .IncludeDirectory("~/Scripts/base", "*.js", true)
                .IncludeDirectory("~/Scripts/extensions", "*.js", true)
                .IncludeDirectory("~/Scripts/modules", "*.js", true)
                .IncludeDirectory("~/Scripts/utility", "*.js", true)
                .IncludeDirectory("~/Scripts/filter", "*.js", true)
                .IncludeDirectory("~/Scripts/control", "*.js", true)
                .IncludeDirectory("~/Scripts/production", "*.js", true)
                );
            // Production Socket
            bundles.Add(new GZipScriptBundle("~/bundles/prod-socket-js")
                .Include("~/Scripts/app-web-socket.js")
                );


            // Non nimify
            var nonMinifyBundle = new ScriptBundle("~/bundles/production-nonminify-js")
                .Include("~/Scripts/helpers/angular-helper.js",
                    "~/Scripts/helpers/div-ready-directive.js");

            nonMinifyBundle.Transforms.Clear();
            bundles.Add(nonMinifyBundle);
        }

        private static void RegisterCssScripts(BundleCollection bundles)
        {
            bundles.Add(new GZipScriptBundle("~/bundles/base-lib-css")
              .Include("~/bower_components/bootstrap/dist/css/bootstrap.min.css", new CssRewriteUrlTransform())
              .Include("~/bower_components/tr-ng-grid/trNgGrid.min.css", new CssRewriteUrlTransform())
              //.Include("~/bower_components/ag-grid/styles/ag-grid.css", new CssRewriteUrlTransform())
              //.Include("~/bower_components/ag-grid/styles/theme-fresh.css", new CssRewriteUrlTransform())
              .Include("~/bower_components/loader/loading.min.css", new CssRewriteUrlTransform())
              .Include("~/bower_components/bootstrap-datepicker/css/bootstrap-datepicker.css", new CssRewriteUrlTransform())
              //.Include("~/bower_components/ui-select/select.min.css", new CssRewriteUrlTransform())
              .Include("~/bower_components/angular-ui-select/select.min.css", new CssRewriteUrlTransform())
              .Include("~/bower_components/angular-ui-select/select2.css", new CssRewriteUrlTransform())
              //.Include("~/bower_components/selectize/css/selectize.default.css", new CssRewriteUrlTransform())
              .Include("~/bower_components/selectize/css/selectize.bootstrap3.css", new CssRewriteUrlTransform())
              );
          
            var styleBundle = new StyleBundle("~/bundles/production-css")
               .Include("~/Content/app.css", new CssRewriteUrlTransform())
               .Include("~/Content/BillWithBarcode.css", new CssRewriteUrlTransform());

            bundles.Add(styleBundle);

            // Custom theme
            RegisterCustomeTheme(bundles);
        }

        private static void RegisterCustomeTheme(BundleCollection bundles)
        {
            RegisterInspiniaTheme(bundles);
        }

        private static void RegisterInspiniaTheme(BundleCollection bundles)
        {
            var contentInspinia = new GZipStyleBundle("~/Content/inspinia")
                .Include("~/fonts/font-awesome/css/font-awesome.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/bootstrap.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/animate.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/style.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/dropdown.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/footable/footable.core.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/iCheck/custom.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/blueimp/css/blueimp-gallery.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/morris/morris-0.4.3.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/dataTables/dataTables.bootstrap.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/dataTables/dataTables.responsive.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/dataTables/dataTables.tableTools.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/jqGrid/ui.jqgrid.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/codemirror/codemirror.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/codemirror/ambiance.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/fullcalendar/fullcalendar.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/ionRangeSlider/ion.rangeSlider.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/ionRangeSlider/ion.rangeSlider.skinFlat.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/datapicker/datepicker3.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/datapicker/angular-datapicker.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/nouslider/jquery.nouislider.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/jasny/jasny-bootstrap.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/switchery/switchery.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/chosen/chosen.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/steps/jquery.steps.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/dropzone/basic.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/dropzone/dropzone.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/toastr/toastr.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/colorpicker/bootstrap-colorpicker.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/cropper/cropper.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/jsTree/style.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/chartist/chartist.min.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/clockpicker/clockpicker.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/daterangepicker/daterangepicker-bs3.css", new CssRewriteUrlTransform())
                .Include("~/Content/themes/inspinia/plugins/sweetalert/sweetalert.css", new CssRewriteUrlTransform())
                .Include("~/Scripts/inspinia/plugins/jquery-ui/jquery-ui.min.css", new CssRewriteUrlTransform());
            //#if DEBUG
            //#else
            //            contentInspinia.Transforms.Add(new LessTransform());
            //            contentInspinia.Transforms.Add(new CssMinify());
            //#endif
            bundles.Add(contentInspinia);
            bundles.Add(new GZipScriptBundle("~/bundles/inspinia")
                .Include(new string[] {
                      "~/Scripts/inspinia/plugins/metisMenu/metisMenu.min.js",
                      "~/Scripts/inspinia/plugins/pace/pace.min.js",
                      "~/Scripts/inspinia/app/inspinia.min.js",
                      "~/Scripts/inspinia/app/skin.config.min.js",
                      "~/Scripts/inspinia/plugins/slimscroll/jquery.slimscroll.min.js",
                      "~/Scripts/inspinia/plugins/peity/jquery.peity.min.js",
                      "~/Scripts/inspinia/plugins/video/responsible-video.js",
                      "~/Scripts/inspinia/plugins/blueimp/jquery.blueimp-gallery.min.js",
                      "~/Scripts/inspinia/plugins/sparkline/jquery.sparkline.min.js",
                      "~/Scripts/inspinia/plugins/morris/raphael-2.1.0.min.js",
                      "~/Scripts/inspinia/plugins/morris/morris.js",
                      "~/Scripts/inspinia/plugins/flot/jquery.flot.js",
                      "~/Scripts/inspinia/plugins/flot/jquery.flot.tooltip.min.js",
                      "~/Scripts/inspinia/plugins/flot/jquery.flot.resize.js",
                      "~/Scripts/inspinia/plugins/flot/jquery.flot.pie.js",
                      "~/Scripts/inspinia/plugins/flot/jquery.flot.time.js",
                      "~/Scripts/inspinia/plugins/flot/jquery.flot.spline.js",
                      "~/Scripts/inspinia/plugins/rickshaw/vendor/d3.v3.js",
                      "~/Scripts/inspinia/plugins/rickshaw/rickshaw.min.js",
                      "~/Scripts/inspinia/plugins/chartjs/Chart.min.js",
                      "~/Scripts/inspinia/plugins/iCheck/icheck.min.js",
                      "~/Scripts/inspinia/plugins/dataTables/jquery.dataTables.js",
                      "~/Scripts/inspinia/plugins/dataTables/dataTables.bootstrap.js",
                      "~/Scripts/inspinia/plugins/dataTables/dataTables.responsive.js",
                      "~/Scripts/inspinia/plugins/dataTables/dataTables.tableTools.min.js",
                      "~/Scripts/inspinia/plugins/jeditable/jquery.jeditable.js",
                      "~/Scripts/inspinia/plugins/jqGrid/i18n/grid.locale-en.js",
                      "~/Scripts/inspinia/plugins/jqGrid/jquery.jqGrid.min.js",
                      "~/Scripts/inspinia/plugins/codemirror/codemirror.js",
                      "~/Scripts/inspinia/plugins/codemirror/mode/javascript/javascript.js",
                      "~/Scripts/inspinia/plugins/nestable/jquery.nestable.js",
                      "~/Scripts/inspinia/plugins/validate/jquery.validate.min.js",
                      "~/Scripts/inspinia/plugins/fullcalendar/moment.min.js",
                      "~/Scripts/inspinia/plugins/fullcalendar/fullcalendar.min.js",
                      "~/Scripts/inspinia/plugins/jvectormap/jquery-jvectormap-1.2.2.min.js",
                      "~/Scripts/inspinia/plugins/jvectormap/jquery-jvectormap-world-mill-en.js",
                      "~/Scripts/inspinia/plugins/ionRangeSlider/ion.rangeSlider.min.js",
                      "~/Scripts/inspinia/plugins/datapicker/bootstrap-datepicker.js",
                      "~/Scripts/inspinia/plugins/datapicker/angular-datepicker.js",
                      "~/Scripts/inspinia/plugins/nouslider/jquery.nouislider.min.js",
                      "~/Scripts/inspinia/plugins/jasny/jasny-bootstrap.min.js",
                      "~/Scripts/inspinia/plugins/switchery/switchery.js",
                      "~/Scripts/inspinia/plugins/chosen/chosen.jquery.js",
                      "~/Scripts/inspinia/plugins/jsKnob/jquery.knob.js",
                      "~/Scripts/inspinia/plugins/staps/jquery.steps.min.js",
                      "~/Scripts/inspinia/plugins/dropzone/dropzone.js",
                      "~/Scripts/inspinia/plugins/toastr/toastr.min.js",
                      "~/Scripts/inspinia/plugins/colorpicker/bootstrap-colorpicker.min.js",
                      "~/Scripts/inspinia/plugins/cropper/cropper.min.js",
                      "~/Scripts/inspinia/plugins/jsTree/jstree.min.js",
                      "~/Scripts/inspinia/plugins/diff_match_patch/javascript/diff_match_patch.js",
                      "~/Scripts/inspinia/plugins/preetyTextDiff/jquery.pretty-text-diff.min.js",
                      "~/Scripts/inspinia/plugins/idle-timer/idle-timer.min.js",
                      "~/Scripts/inspinia/plugins/tinycon/tinycon.min.js",
                      "~/Scripts/inspinia/plugins/chartist/chartist.min.js",
                      "~/Scripts/inspinia/plugins/clockpicker/clockpicker.js",
                      "~/Scripts/inspinia/plugins/fullcalendar/moment.min.js",
                      "~/Scripts/inspinia/plugins/daterangepicker/daterangepicker.js",
                      "~/Scripts/inspinia/plugins/daterangepicker/angular-daterangepicker.js",
                      "~/Scripts/inspinia/plugins/sweetalert/sweetalert.min.js",
                      "~/Scripts/inspinia/plugins/footable/footable.all.min.js",
                      "~/Scripts/inspinia/plugins/jquery-ui/jquery-ui.min.js"})
                );
        }
    }
}
