const path = require("path");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");

module.exports = {
    devtool: "source-map",
    mode: "development",
    entry: {
        main: "./assets/js/main.js"
    },
    output: {
        filename: "[name].js",
        path: path.resolve(__dirname, "wwwroot/assets/js"),
    },
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: "babel-loader",
                    options: {
                        presets: ["@babel/preset-env"],
                    },
                },
            },
            {
                test: /\.(sa|sc|c)ss$/,
                exclude: /node_modules/,
                use: [MiniCssExtractPlugin.loader, "css-loader", "sass-loader"],
            },
            {
                test: /\.(jpg|png|gif)$/,
                type: 'asset/resource',
                dependency: { not: ['url'] },
            },
            {
                test: /\.(woff2?|ttf|otf|eot|svg)$/,
                type: 'asset/resource',
                dependency: { not: ['url'] },
            }
        ],
    },
    plugins: [
        new MiniCssExtractPlugin({
            filename: "../css/main.css",
        })
    ],
    stats: {
        errorDetails: true,
    }
};