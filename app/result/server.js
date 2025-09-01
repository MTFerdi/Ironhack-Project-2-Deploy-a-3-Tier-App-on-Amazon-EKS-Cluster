const express = require("express");
const { Pool } = require("pg");
const app = express();

const pool = new Pool({
  host: process.env.PGHOST || "db",
  port: process.env.PGPORT || 5432,
  database: process.env.PGDATABASE || "votes",
  user: process.env.PGUSER || "postgres",
  password: process.env.PGPASSWORD || "postgres"
});

app.get("/", async (req,res) => {
  try {
    const { rows } = await pool.query("SELECT vote, COUNT(*) AS c FROM votes GROUP BY vote");
    res.send(JSON.stringify(rows));
  } catch (e) {
    res.status(500).send(e.toString());
  }
});

app.listen(80, () => console.log("result on :80"));
