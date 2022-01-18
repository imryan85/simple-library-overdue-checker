const cron = require('node-cron');
const axios = require('axios');

require('dotenv').config()

cron.schedule('* * * * *', () => {
  console.log('every minute log ' + new Date());
});

cron.schedule('0 0 * * *', async () => {
  console.log('running every midnight');

  // get all overdue books (booklended ids)
  const { data } = await axios.get(`${process.env.SVC_API_URI}/overdue/all`);

  console.log('Overdue books. (booklended ids)');
  console.log(data);

  // put it in overdue queue in subsystem
  data.forEach(bookLended => {
    const bookLendedId = bookLended._id;
    console.log(`pushing to the queue. bookLendedId: ${bookLendedId}`);
    axios.post(`${process.env.BULL_MQ_URI}/queue/handleOverdue`, { bookLendedId });
  });
});