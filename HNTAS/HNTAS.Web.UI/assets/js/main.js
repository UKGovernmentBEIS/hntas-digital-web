// const GOVUKFrontend = require('./govuk-frontend-all.js');
import "../scss/main.scss";

import { initAll } from 'govuk-frontend'
initAll()

// // overriding to allow js on mobile, this must run before the initAll() method  
// GOVUKFrontend.Tabs.prototype.setupResponsiveChecks = function () {
//     this.setup();
// };

// GOVUKFrontend.initAll();