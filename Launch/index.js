const process = require("process");
const child_process = require("child_process");

var path = process.argv[2];

var deleg = (x) => console.log(x.toString());
var hookOut = (x) => x.on("data", deleg);
var hookErr = (x) => hookOut(x.stderr) && hookOut(x.stdout);

var build = child_process.spawn("dotnet", ["build", path + "/DiscordBot/src"]);
hookErr(build);

var UntilExit = new Promise((OnGood, OnBad) => {
  build.once("exit", (x) => (x == 0 ? OnGood : OnBad)(x));
});

UntilExit.catch((e) => {
  throw e;
});

UntilExit.then((OnGood, re) => {
  ///Start bot process
  var run = child_process.spawn("dotnet", [
    "run",
    "--project",
    path + "/DiscordBot/src/bot-core",
  ]);

  ///Log std err/out for data
  hookErr(run);

  /// Add linking events to prevent leaving zombie processes
  run.once("exit", () => OnGood());
  var ex = () => run.kill();
  process.once("SIGTERM", ex);
  process.once("SIGINT", ex);
  process.once("SIGHUP", ex);
}).catch((e) => {
  throw e;
});
