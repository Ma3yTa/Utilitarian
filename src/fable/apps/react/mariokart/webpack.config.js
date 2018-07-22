var path = require("path");
var webpack = require("webpack");
var fableUtils = require("fable-utils");
var HtmlWebpackPlugin = require('html-webpack-plugin');
var HtmlWebpackPolyfillIOPlugin = require('html-webpack-polyfill-io-plugin');

var isProduction = process.argv.indexOf("-p") >= 0;
var port = process.env.SUAVE_FABLE_PORT || "8085";

function resolve(filePath) {
  return path.join(__dirname, filePath)
}

var filePath = {
  build: resolve("./build"),
  node_modules : resolve("./node_modules"),
  /** path to html template file, where we can specify the html skeleton around our app.
  This template can be enriched at buildtime by the webpack-html-plugin, e.g. by inserting
  references to the generated bundle or inclusion of polyfills */
  indexTemplate : resolve("./index.html"),
  fsproj : resolve("./Utilitarian.Apps.Mariokart.React.fsproj")
}

/** configuration elements needed for both production and development builds */
var commonConfig = {
  
  /** this represents the entry point of bundle generation with webpack.
    That means somewhere in the chain of loaders, one of them needs 
    to know what to do with this entry point. In order for webpack to know 
    the correct loader, we specify a set of rules that match on file extensions.
    Unsuprisingly, .fs, .fsx and .fsproj files are handled by a fable-loader. */
  entry: filePath.fsproj,
  
  /** this controls the name and location of the generated bundle */
  output: {
    path: filePath.build,
    filename: "public/bundle.js"
  },
  
  /** this captures all locations of js modules to look into when resolving imports during 
    the bundling process */
  resolveModules : {
    modules: [ filePath.node_modules ]
  },
}

/** configuration elements needed for development builds only */
var devConfig = {

  /** source-map generation tool. Debuggers need generated source maps
    in order to track down executing js code to .fs file locations */ 
  sourceMapTool: "source-map",
  
  /** configuration of webpack-dev-server */
  devServer: {
    proxy: {
      '/api/*': {
        target: 'http://localhost:' + port,
        changeOrigin: true
      }
    },
    hot: true,
    inline: true,
    historyApiFallback: true,
  },
  /** rules to use for development builds */
  moduleRules : getModuleRules(false),
  /** plugins to use for development builds */
  plugins : getPlugins(false)
}

/** configuration elements needed for production builds only */
var prodConfig = {
  /** rules to use for production builds */  
  moduleRules : getModuleRules(true),
  /** plugins to use for production builds */
  plugins : getPlugins(true)
}

/**
 * yields all module rules to be used by webpack
 * @param {*} isProduction flag that states whether we are in a development or production build
 */
function getModuleRules(isProduction) {
  
  // #region RULE DECLARATIONS
  
  var babelOptions = fableUtils.resolveBabelOptions({
    presets: [
      ["env", { "targets": { "browsers": "> 1%" }, "modules": false }]
    ],
  });

  var fsharpFilesRule = {
    test: /\.fs(x|proj)?$/,
    use: {
      loader: "fable-loader",
      options: {
        babel: babelOptions,
        define: isProduction ? [] : ["DEBUG"]
      }
    }
  }

  var lessFilesRule = {
    test: /\.less$/,
    use: [{
        loader: "style-loader"
    }, {
        loader: "css-loader"
    }, {
        loader: "less-loader",
        options: {
            javascriptEnabled: true
        }
    }]
  }

  var jsFilesRule = {
    test: /\.js$/,
    exclude: /node_modules/,
    use: {
      loader: 'babel-loader',
      options: babelOptions
    },
  }
  //#endregion
  
  return isProduction ? 
  
    [
      fsharpFilesRule,
      lessFilesRule,
      jsFilesRule
    ] 
    
    : 
  
    [
      fsharpFilesRule,
      lessFilesRule,
      jsFilesRule
    ]
}

/**
 * yields all plugins to be used by webpack
 * @param {*} isProduction flag that states whether we are in a development or production build
 */
function getPlugins(isProduction) {
  
  // #region PLUGIN DECLARATIONS
  var webpackHtmlPlugin = new HtmlWebpackPlugin(
    {
      hash: isProduction,
      template: filePath.indexTemplate,
    }
  )

  var polyFillPlugin = new HtmlWebpackPolyfillIOPlugin(
    { features: "es6,fetch" }
  )
  // #endregion

  return isProduction ? 
  
    [
      webpackHtmlPlugin,
      polyFillPlugin,
    ]

    :

    [
      webpackHtmlPlugin,
      polyFillPlugin,
      new webpack.HotModuleReplacementPlugin(),
      new webpack.NamedModulesPlugin()
    ]  
}

console.log("Bundling for " + (isProduction ? "production" : "development") + " ...");
/** this is the export of the configuration in the correct format 
  for being processed by webpack */
module.exports = 
  isProduction ? 
    {
      entry: commonConfig.entry,
      output : commonConfig.output,
      resolve: commonConfig.resolveModules,

      module: {
        rules : prodConfig.moduleRules
      },

      plugins: prodConfig.plugins

    } 

    :
    
    {
      devtool: devConfig.sourceMapTool,
      entry: commonConfig.entry,
      output : commonConfig.output,
      resolve: commonConfig.resolveModules,
      devServer : devConfig.devServer,
      
      module: {
        rules : devConfig.moduleRules
      },
      plugins: devConfig.plugins
    }
