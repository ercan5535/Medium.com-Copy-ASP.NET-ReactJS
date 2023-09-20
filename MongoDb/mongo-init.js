const databaseName = process.env.MONGO_INITDB_DATABASE
const collectionName = process.env.MONGO_INITDB_COLLECTION

db = db.getSiblingDB(databaseName);
db.createCollection(collectionName);