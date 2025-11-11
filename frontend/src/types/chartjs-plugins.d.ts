// This line can often be enough if the plugin ships with its own augmentations
import 'chartjs-plugin-datalabels';

// If the above import is not sufficient, you might need more explicit augmentation:
// import { ChartType, PluginOptions } from 'chart.js';
// import { Options as DataLabelsOptions } from 'chartjs-plugin-datalabels';

// declare module 'chart.js' {
//   interface PluginOptionsByType<TType extends ChartType> {
//     datalabels?: DataLabelsOptions;
//   }
// }
