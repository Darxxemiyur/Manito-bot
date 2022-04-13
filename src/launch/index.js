const process = require("process");
const child_process = require("child_process");

var path = process.argv[2];

var build = child_process.spawn("dotnet", ["build", path]);
build.stdout.on("data", (x) => console.log(x.toString()));

var UntilExit = new Promise((OnGood, OnBad) => {
  build.once("exit", (x) => (x == 0 ? OnGood : OnBad)(x));
});

UntilExit.then((OnGood) => {
  ///Start bot process
  var run = child_process.spawn("dotnet", [
    "run",
    "--project",
    path + "/bot-core",
  ]);

  ///Log std err/out for data
  run.stdout.on("data", (x) => console.log(x.toString()));
  run.stdout.on("error", (x) => console.log(x.toString()));
  run.stderr.on("data", (x) => console.log(x.toString()));
  run.stderr.on("error", (x) => console.log(x.toString()));

  /// Add linking events to prevent leaving zombie processes
  run.once("exit", () => OnGood());
  var ex = () => run.kill();
  process.once("SIGTERM", ex);
  process.once("SIGINT", ex);
  process.once("SIGHUP", ex);
}).catch((e) => {
  throw e;
});
